using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2DPointEmitter : P2DEmitter
{
    public override P2DEmitterType type { get { return P2DEmitterType.Point; } }

    public override P2DParticle Emit(P2DParticleSystem p, Vector2 position)
    {
        P2DParticle ret = new P2DParticle();

        float speed = (p.startSpeed + Util.signedRandomValue * p.speedVariation);

        ret.position = position;
        ret.velocity = Random.insideUnitCircle.normalized * speed;
        ret.angle = p.startAngle + Util.signedRandomValue * p.angleVariation;
        ret.scale = p.startScale + Util.signedRandomValue * p.scaleVariation;
        ret.angularVelocity = p.startAngularVelocity + Util.signedRandomValue * p.angularVelocityVariation;
        ret.alpha = p.startAlpha + Util.signedRandomValue * p.alphaVariation;
        ret.life = p.life;
        ret.birth = Time.time;

        if (p.angleRelativeToVelocity && ret.velocity.magnitude > 0)
        {
            float angle = Vector2.Angle(ret.velocity, Vector2.up);

            if (ret.velocity.x < 0)
                ret.angle += angle;
            else
                ret.angle -= angle;
        }

        return ret;
    }
}
