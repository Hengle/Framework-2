﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {

    [System.Serializable]
    public class WatchTarget {
        [SerializeField] private int _target;
        public int LastSensedTurnCount;
        public Point3 LastSensedPos;
        public bool Seen;
        public CharacterTemplate Target {
            get { return EntityController.Get(_target).GetTemplate<CharacterTemplate>(); }
        }

        public WatchTarget(Entity target) {
            _target = target;
        }
    }

    public static class ActorWatchTargetExtension {
        public static bool ContainsUnit(this List<WatchTarget> list, Entity unit) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Target.Entity == unit) {
                    return true;
                }
            }
            return false;
        }

        public static WatchTarget GetUnit(this List<WatchTarget> list, Entity unit) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Target.Entity == unit) {
                    return list[i];
                }
            }
            return null;
        }

        public static void Sort(this List<WatchTarget> list, Point3 pos) {
            for (int write = 0; write < list.Count; write++) {
                for (int sort = 0; sort < list.Count - 1; sort++) {
                    if (list[sort].LastSensedPos.SqrDistance(pos) >
                        list[sort + 1].LastSensedPos.SqrDistance(pos)) {
                        var lesser = list[sort + 1];
                        list[sort + 1] = list[sort];
                        list[sort] = lesser;
                    }
                }
            }
        }
    }
}
