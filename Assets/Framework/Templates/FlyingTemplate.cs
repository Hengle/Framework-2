﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class FlyingTemplate : BaseTemplate {

        private CachedComponent<FlightEngine> _flight = new CachedComponent<FlightEngine>();
        private CachedComponent<FakeFlightEngine> _fakeFlight = new CachedComponent<FakeFlightEngine>();
        private CachedComponent<HoverEngine> _hover = new CachedComponent<HoverEngine>();
        private CachedComponent<FlightControl> _control = new CachedComponent<FlightControl>();
        private CachedComponent<CosmeticFlightBanking> _banking = new CachedComponent<CosmeticFlightBanking>();
        private CachedComponent<RigidbodyComponent> _rigidBody = new CachedComponent<RigidbodyComponent>();
        private CachedComponent<FlightPlayerInput> _playerInput = new CachedComponent<FlightPlayerInput>();
        private CachedComponent<FlightMoveInput> _flightMoveInput = new CachedComponent<FlightMoveInput>();
        private CachedComponent<SimpleProjectileSpawner> _projectile = new CachedComponent<SimpleProjectileSpawner>();
        private CachedComponent<TransformComponent> _tr = new CachedComponent<TransformComponent>();
        private CachedComponent<SteeringInput> _moveInput = new CachedComponent<SteeringInput>();
        public FlightEngine Engine => _flight.Value;
        public FakeFlightEngine FakeFlight => _fakeFlight.Value;
        public HoverEngine Hover => _hover.Value;
        public TransformComponent Tr => _tr.Value;
        public FlightControl Control => _control.Value;
        public CosmeticFlightBanking Banking => _banking.Value;
        public Rigidbody Rigidbody => _rigidBody.Value.Rb;
        public FlightPlayerInput PlayerInput => _playerInput.Value;
        public FlightMoveInput FlightMoveInput => _flightMoveInput.Value;
        public SimpleProjectileSpawner Projectile => _projectile.Value;
        public SteeringInput SteeringInput => _moveInput.Value;

        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _flight, _control, _rigidBody, _fakeFlight, _hover, _banking, _playerInput, _flightMoveInput, _projectile, _tr,_moveInput
        };

        public override System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(FlightControl),
                typeof(RigidbodyComponent)
            };
        }

        public void SetEngineActivation(bool setActivated) {
            Control.EnginesActivated = setActivated;
            if (!Control.EnginesActivated) {
                Control.ClearValues();
            }
        }

        public void UpdateControl() {
            if (PlayerInput != null) {
                PlayerInput.UpdateControl(Control);
            }
            if (FlightMoveInput != null) {
                FlightMoveInput.UpdateControl(Control);
            }
        }
    }

    public class NpcFlyingTemplate : FlyingTemplate {

        private const int WanderRadius = 20;
        
        private CachedComponent<MoveTarget> _moveTarget = new CachedComponent<MoveTarget>();
        private CachedComponent<SensorTargetsComponent> _sensorTargets = new CachedComponent<SensorTargetsComponent>();

        private Timer _wanderTimer = new Timer(6f, false);

        public bool Chasing = false;
        public MoveTarget MoveTarget => _moveTarget.Value;
        public SensorTargetsComponent SensorTargets => _sensorTargets.Value;
        

        public override List<CachedComponent> GatherComponents {
            get {
                var baseList = base.GatherComponents;
                baseList.Add(_moveTarget);
                baseList.Add(_sensorTargets);
                return baseList;
            }
        }

        public bool TryWander() {
            if (_wanderTimer.IsActive) {
                return true;
            }
            Chasing = false;
            var position = World.Get<PathfindingSystem>().FindOpenPosition(Tr.position, WanderRadius);
            if (position == Tr.position) {
                return false;
            }
            _wanderTimer.Restart();
            Entity.Post(new SetMoveTarget(Entity, null, position));
            return true;
        }

        public void Stop() {
            MoveTarget.ClearMove();
            Entity.Tags.Remove(EntityTags.Moving);
        }

        public bool Chase(CharacterTemplate target) {
            //var gridPosition = target.Get<GridPosition>().Position;
            //if (gridPosition.IsNeighbor(Position.c.Position)) {
            //    return false;
            //}
            if (!World.Get<FactionSystem>().AreEnemies(Entity, target)) {
                return false;
            }
            Chasing = true;
            Entity.Post(new SetMoveTarget(Entity, target.Tr, null));
            Entity.Post(new SetLookTarget(Entity, target, false));
#if DEBUG
            DebugLog.Add(Entity.DebugId + " chasing " + target.Entity.DebugId);
#endif
            return true;
        }

        public void StopFaceTarget(Entity target) {
            Stop();
            if (target == Entity) {
                return;
            }
            Entity.Post(new SetLookTarget(Entity, target, true));
        }

        public static new System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(FlightControl),
                typeof(RigidbodyComponent),
                typeof(SimpleProjectileSpawner),
                typeof(SensorTargetsComponent),
            };
        }
    }
}
