using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public sealed class BlockDamageNode : StateGraphNode {

        public float WaitTime;
        
        public override bool DrawGui(GUIStyle textStyle, GUIStyle buttonStyle) {
            return false;
        }

        public override string Title { get { return "Node"; } }

        public override RuntimeStateNode GetRuntimeNode(RuntimeStateGraph graph) {
            return new RuntimeNode(this, graph);
        }

        public class RuntimeNode : RuntimeStateNode {
            
            private RuntimeStateNode _exitNode;
            private BlockDamageNode _originalNode;
            private BlockDamageAction _config;
            private ActionUsingTemplate _owner;
            private VitalStat _vitalStat;
            private bool _isWaiting;
            private ActionFx _fxComponent;
            private float _finalCost;
            
            public override RuntimeStateNode GetExitNode() {
                return _exitNode;
            }

            public RuntimeNode(BlockDamageNode node, RuntimeStateGraph graph) : base(node,graph) {
                _exitNode = GetOriginalNodeExit();
                _originalNode = node;
            }

            public override void OnEnter(RuntimeStateNode lastNode) {
                base.OnEnter(lastNode);
                _owner = Graph.Entity.GetTemplate<ActionUsingTemplate>();
                _config = Graph.Entity.Get<BlockDamageAction>();
                var model = ItemPool.Spawn(UnityDirs.Models, _config.ModelData, Vector3.zero, Quaternion.identity);
                if (model != null) {
                    _owner.ParentSpawn(model.Transform);
                    _owner.ActionEvent.Action.Entity.Add(new RenderingComponent(model.GetComponent<IRenderingComponent>()));
                    _owner.ActionEvent.Action.Entity.Add(new TransformComponent(model.transform));
                }
                var dmgComponent = Graph.Entity.GetOrAdd<BlockDamage>();
                if (!Graph.Entity.Tags.Contain(EntityTags.Player)) {
                    dmgComponent.DamageBlockers.Add(BlockDamageFlat);
                    _isWaiting = true;
                    _vitalStat = null;
                    return;
                }
                _vitalStat = _owner.Stats.GetVital(_config.TargetVital);
                var skillMulti = 1f;
                if (!string.IsNullOrEmpty(_config.Skill)) {
                    var skillValue = _owner.Entity.FindStatValue(_config.Skill);
                    skillMulti = Mathf.Clamp(1 - (skillValue * CostVital.SkillPercent), CostVital.SkillMaxReduction, 1);
                }
                dmgComponent.CollisionHandlers.Add(EvadeDamageWithStats);
                _fxComponent = _owner.ActionEvent.Action.Fx;
                _finalCost = _config.Cost * skillMulti;
            }

            public override void OnExit() {
                base.OnExit();
                _vitalStat = null;
                var tr = _owner.ActionEvent.Action.Entity.Get<TransformComponent>();
                if (tr != null) {
                    ItemPool.Despawn(tr.gameObject);
                    _owner.ActionEvent.Action.Entity.Remove(tr);
                    _owner.ActionEvent.Action.Entity.Remove<RenderingComponent>();
                }
                var blockDamage = _owner.Entity.Get<BlockDamage>();
                if (blockDamage != null) {
                    if (_isWaiting) {
                        blockDamage.DamageBlockers.Remove(BlockDamageFlat);
                    }
                    else {
                        blockDamage.CollisionHandlers.Remove(EvadeDamageWithStats);
                    }
                }
            }

            public override bool TryComplete(float dt) {
                if (_isWaiting) {
                    if (TimeManager.Time >= _owner.ActionEvent.TimeStart + _originalNode.WaitTime) {
                        return true;
                    }
                    return false;
                }
                if (!PlayerInputSystem.GetButton(_config.ChargeInput) || _vitalStat.Current < _config.Cost) {
                    return true;
                }
                return false;
            }

            private bool BlockDamageFlat(TakeDamageEvent dmgEvent) {
                return true;
            }

            private int EvadeDamageWithStats(CollisionEvent arg) {
                if (_vitalStat == null) {
                    return arg.Hit;
                }
                if (_fxComponent != null) {
                    _fxComponent.TriggerEvent(
                        new ActionEvent(
                            arg.Origin, arg.Target, arg.HitPoint, Quaternion.LookRotation(arg.HitNormal),
                            ActionState.Collision));
                }

                _vitalStat.Current -= _finalCost;
                return CollisionResult.Miss;
            }
        }
    }
}
