using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class P2DEmitter
{
    public abstract P2DEmitterType type { get; }
    public abstract P2DParticle Emit(P2DParticleSystem particleSystem, Vector2 position);
}
