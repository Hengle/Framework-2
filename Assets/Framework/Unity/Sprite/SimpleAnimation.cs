﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace PixelComrades {
    public class SimpleAnimation : SpriteAnimation {
        
        [SerializeField, FormerlySerializedAs("Sprites")] private Sprite[] _sprites;
        [SerializeField, FormerlySerializedAs("Colliders")] private SavedSpriteCollider[] _colliders;
        public override int LengthSprites { get { return _sprites.Length; } }
        public override Sprite[] Sprites { get => _sprites; set => _sprites = value; }
        public override SavedSpriteCollider[] Colliders { get => _colliders; set => _colliders = value; }
    }
}
