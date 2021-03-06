﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PixelComrades {
    [System.Serializable]
	public sealed class InventoryItem : IComponent, IDisposable {
        public InventoryItem(int maxStack, int price, int rarity) {
            MaxStack = maxStack;
            Price = price;
            Rarity = rarity;
        }

        public InventoryItem(SerializationInfo info, StreamingContext context) {
            MaxStack = info.GetValue(nameof(MaxStack), MaxStack);
            Price = info.GetValue(nameof(Price), Price);
            Rarity = info.GetValue(nameof(Rarity), Rarity);
            Index = info.GetValue(nameof(Index), Index);
            Count = info.GetValue(nameof(Count), Count);
            Identified = info.GetValue(nameof(Identified), Identified);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue(nameof(MaxStack), MaxStack);
            info.AddValue(nameof(Price), Price);
            info.AddValue(nameof(Rarity), Rarity);
            info.AddValue(nameof(Index), Index);
            info.AddValue(nameof(Count), Count);
            info.AddValue(nameof(Identified), Identified);
        }
        
        public int MaxStack { get; }
        public int Price { get; }
        public int Rarity { get; }

        public int Index = -1;
        public int Count = 1;
        public bool Identified = true;
        
        private CachedComponent<ItemInventory> _inventory = new CachedComponent<ItemInventory>();
        public IEntityContainer Inventory { get { return _inventory.Value; } }

        public void SetContainer(IEntityContainer container) {
            if (container == null) {
                _inventory.Clear();
            }
            else {
                _inventory.Set(container.Owner);
            }
        }

        public void Dispose() {
            if (_inventory.Value != null) {
                _inventory.Value.Remove(this.GetEntity());
            }
        }
    }
}
