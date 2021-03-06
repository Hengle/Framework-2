using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
    public sealed class WeaponBobComponent : IComponent {
        
        public Transform ArmsPivot = null;
        public Vector3 ResetPoint;
        public float BobTime = 0;
        

        public WeaponBobComponent(Transform pivot ) {
            ArmsPivot = pivot;
            ResetPoint = ArmsPivot.localPosition;

        }
        
        public WeaponBobComponent(SerializationInfo info, StreamingContext context) {
            //BuildingIndex = info.GetValue(nameof(BuildingIndex), BuildingIndex);
        }
                
        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            //info.AddValue(nameof(BuildingIndex), BuildingIndex);
        }
    }
}
