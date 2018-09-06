﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public class ShadowFloodFill {

        public static void GetVisiblePoints(Point3 start, int maxRowDistance, Action<BaseCell> del, Func<BaseCell, bool> ignoreCheck) {
            if (World.Get<MapSystem>().GetCell(start) == null) {
                return;
            }
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(0, 0, 1), new Point3(0, 0, -1) }, new Point3(-1,0,0));
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(0, 0, 1), new Point3(0, 0, -1) }, new Point3(1, 0, 0));
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(1, 0, 0), new Point3(-1, 0, 0) }, new Point3(0, 0, 1));
            CheckRow(start, start, maxRowDistance, del, ignoreCheck, new[] { new Point3(1, 0, 0), new Point3(-1, 0, 0) }, new Point3(0, 0, -1));
        }

        public static void CheckRow(Point3 origin, Point3 checkStart, int maxRowDistance, Action<BaseCell> del, Func<BaseCell, bool> ignoreCheck, Point3[] adjacent, Point3 increment) {
            var currCell = World.Get<MapSystem>().GetCell(checkStart);
            if (currCell == null) {
                return;
            }
            del(currCell);
            for (int i = 1; i < maxRowDistance; i++) {
                var pos = checkStart + (increment * i);
                if (currCell.BlocksVision(currCell.WorldPosition.GetTravelDirTo(pos).ToDirectionEight())) {
                    continue;
                }
                currCell = World.Get<MapSystem>().GetCell(pos);
                if (currCell == null || ignoreCheck(currCell)) {
                    break;
                }
                del(currCell);
                if (origin.Distance(pos) > maxRowDistance) {
                    break;
                }
                for (int a = 0; a < adjacent.Length; a++) {
                    var adjPos = pos + adjacent[a];
                    if (currCell.BlocksVision(pos.GetTravelDirTo(adjPos).ToDirectionEight())) {
                        continue;
                    }
                    CheckRow(origin, adjPos, maxRowDistance, del, ignoreCheck, new[]{ increment, increment * -1}, adjacent[a]);
                }
            }
        }
    }
}