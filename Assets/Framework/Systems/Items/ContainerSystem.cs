using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    [AutoRegister]
    public sealed class ContainerSystem : SystemBase  {
        
        private CircularBuffer<ActionStateEvent> _eventLog = new CircularBuffer<ActionStateEvent>(10, true);

        public ContainerSystem() {
            NodeFilter<ContainerItemNode>.Setup(ContainerItemNode.GetTypes());
        }
//
//        public void HandleGlobal(EntityDestroyed arg) {
//            var node = arg.Entity.GetNode<ContainerItemNode>();
//            if (node != null && node.Item.Inventory != null) {
//                node.Item.Inventory.Remove(arg.Entity);
//                node.Item.SetContainer(null);
//            }
//        }
    }

    public class ContainerItemNode : BaseNode {

        private CachedComponent<InventoryItem> _item = new CachedComponent<InventoryItem>();
        
        public InventoryItem Item { get => _item.Value; }
        
        public override List<CachedComponent> GatherComponents => new List<CachedComponent>() {
            _item, 
        };

        public static System.Type[] GetTypes() {
            return new System.Type[] {
                typeof(InventoryItem),
            };
        }
    }
    
}