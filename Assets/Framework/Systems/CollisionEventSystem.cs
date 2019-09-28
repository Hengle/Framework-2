﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [Priority(Priority.High), AutoRegister]
    public class CollisionEventSystem : SystemBase, IReceiveGlobal<CollisionEvent>,
        IReceive<EnvironmentCollisionEvent>, IReceive<PerformedCollisionEvent> {

        public List<ICollisionHandler> Handlers = new List<ICollisionHandler>();
        
        private GameOptions.CachedBool _collisionMessage = new GameOptions.CachedBool("CollisionMessages");
        private FastString _collisionString = new FastString();
        private CircularBuffer<CollisionEvent> _eventLog = new CircularBuffer<CollisionEvent>(10, true);

        public CollisionEventSystem() {
            EntityController.RegisterReceiver(new EventReceiverFilter(this, new[] {
                typeof(DespawnOnCollision)
            }));
        }

        [Command("PrintCollisionLog")]
        public static void PrintLog() {
            var log = World.Get<CollisionEventSystem>()._eventLog;
            foreach (var msg in log.InOrder()) {
                Console.Log(string.Format("{6}: {0} hit {1} at {2} eventNum {3} normal {4} source {5}",
                    msg.Origin?.Entity.DebugId ?? "null",
                    msg.Target?.Entity.DebugId ?? "null",
                    msg.HitPoint, msg.Hit, msg.HitNormal, msg.Source.DebugId, log.GetTime(msg)));
            }
        }

        public void HandleGlobal(CollisionEvent msg) {
            _eventLog.Add(msg);
            if (msg.Hit < 0) {
                msg.Hit = 10;
                for (int h = 0; h < Handlers.Count; h++) {
                    msg.Hit = MathEx.Min(Handlers[h].CheckHit(msg), msg.Hit);
                }
            }
            if (msg.Hit <= 0) {
                return;
            }
            var actionStateEvent = new ActionStateEvent(msg.Origin.Entity, msg.Target.Entity, msg.HitPoint,
                msg.HitNormal == Vector3.zero ? Quaternion.identity :Quaternion.LookRotation(msg.HitNormal), 
            ActionStateEvents.Impact);
            msg.Source.PostAll(new ImpactEvent(msg, msg.Origin.Entity.FindNode<CharacterNode>(), msg.Target.Entity.FindNode<CharacterNode>
            ()));
            msg.Origin.Post(actionStateEvent);
            if (!_collisionMessage) {
                return;
            }
            _collisionString.Clear();
            _collisionString.Append(msg.Origin.GetName());
            _collisionString.Append(" struck ");
            _collisionString.Append(msg.Target.GetName());
            MessageKit<UINotificationWindow.Msg>.post(Messages.MessageLog, new UINotificationWindow.Msg(
                _collisionString.ToString(),"", Color.blue));
            if (msg.Target.Entity.HasComponent<DespawnOnCollision>()) {
                msg.Target.Entity.Destroy();
            }
        }

        public void Handle(EnvironmentCollisionEvent msg) {
            if (msg.EntityHit.HasComponent<DespawnOnCollision>()) {
                msg.EntityHit.Destroy();
            }
        }

        public void Handle(PerformedCollisionEvent msg) {
            if (msg.Origin.Entity.HasComponent<DespawnOnCollision>()) {
                msg.Origin.Entity.Destroy();
            }
        }
    }

    public struct ImpactEvent : IEntityMessage {
        public Entity Source { get; }
        public CharacterNode Origin { get; }
        public CharacterNode Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public int Hit { get; }

        public ImpactEvent(CollisionEvent collisionEvent, CharacterNode origin, CharacterNode target) {
            Source = collisionEvent.Source;
            Origin = origin;
            Target = target;
            HitPoint = collisionEvent.HitPoint;
            HitNormal = collisionEvent.HitNormal;
            Hit = collisionEvent.Hit;
        }
    }

    [Priority(Priority.Higher)]
    public struct EnvironmentCollisionEvent : IEntityMessage {
        public Entity EntityHit { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }

        public EnvironmentCollisionEvent(Entity entityHit, Vector3 hitPoint, Vector3 hitNormal) {
            EntityHit = entityHit;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
        }
    }

    [Priority(Priority.Higher)]
    public struct PerformedCollisionEvent : IEntityMessage {

        
        public CollidableNode Origin { get; }
        public CollidableNode Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public int Hit { get; }
        public PerformedCollisionEvent(CollidableNode origin, CollidableNode target, Vector3 hitPoint, Vector3 hitNormal, int 
        hit = -1) {
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }
    }

    [Priority(Priority.Higher)]
    public struct CollisionEvent : IEntityMessage {
        public Entity Source { get; }
        public CollidableNode Origin { get; }
        public CollidableNode Target { get; }
        public Vector3 HitPoint { get; }
        public Vector3 HitNormal { get; }
        public int Hit;
        public CollisionEvent(Entity source, CollidableNode origin, CollidableNode target, Vector3 hitPoint, Vector3 hitNormal, int hit = -1) {
            Source = source;
            Origin = origin;
            Target = target;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            Hit = hit;
        }
    }

    public interface ICollisionHandler {
        int CheckHit(CollisionEvent collisionEvent);
    }

    public class CollisionResult : GenericEnum<CollisionResult, int> {
        public static int Miss = 0;
        public static int Graze = 5;
        public static int Hit = 10;
        public static int CriticalHit = 15;

        public override int Parse(string value, int defaultValue) {
            if (int.TryParse(value, out var val)) {
                return val;
            }
            return TryValueOf(value, out val) ? val : defaultValue;
        }
    }
}
