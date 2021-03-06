using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelComrades {
    public interface IRuntimeSequenceObject : IDisposable {
        float StartTime { get; }
        float EndTime { get; }
        void OnEnter();
        void OnUpdate(float dt);
        void OnExit();
    }
}
