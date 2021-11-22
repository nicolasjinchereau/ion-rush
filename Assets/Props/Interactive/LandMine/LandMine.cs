using UnityEngine;
using System.Collections;

public class LandMine : MonoBehaviour
{
    public MeshRenderer bodyRenderer;
    public MeshRenderer indicatorRenderer;
    public ParticleSystem explosion;

    Vector3 startPos;
    float hoverHeight = 0.7f;
    float maxSpinSpeed = 360.0f;
    float spinSpeedspinSpeed = 0;
    bool detonated = false;

    void Awake()
    {
        startPos = transform.position;
        indicatorRenderer.material = new Material(indicatorRenderer.sharedMaterial);
    }

    float armingStatus = 0;

    void Update()
    {
        if(!GameController.state.isRunning)
            return;

        if (!detonated)
        {
            bool playerInRange = false;

            var playerPos = Player.position + Vector3.up * 0.5f;

            if(playerPos.y >= startPos.y - 0.5f
            && playerPos.y <= startPos.y + hoverHeight + 0.26f)
            {
                var dist = Vector2.Distance(
                    new Vector2(playerPos.x, playerPos.z),
                    new Vector2(startPos.x, startPos.z));

                if(dist <= Difficulty.landMineRange)
                    playerInRange = true;
            }

            if (playerInRange)
            {
                if (armingStatus < 1.0f - float.Epsilon)
                {
                    armingStatus = Mathf.Min(armingStatus + Time.deltaTime / Difficulty.landMineArmingTime, 1.0f);
                }
                else
                {
                    detonated = true;

                    bodyRenderer.enabled = false;
                    indicatorRenderer.enabled = false;

                    explosion.Play();
                    SharedSounds.explosion.Play();
                    SharedSounds.shortCircuit.Play();
                    Player.DoExplosiveDamage(transform.position, Difficulty.landMineDamage, Difficulty.landMineRange, BatteryDrainReason.Landmine);
                }
            }
            else
            {
                armingStatus = Mathf.Max(armingStatus - Time.deltaTime / Difficulty.landMineArmingTime, 0);
            }

            transform.position = startPos + Vector3.up * armingStatus * hoverHeight;
            indicatorRenderer.material.SetColor("_Color", new Color(armingStatus, armingStatus, armingStatus, 1));
            transform.Rotate(0, maxSpinSpeed * Curve.OutDecInv(armingStatus) * Time.deltaTime, 0, Space.World);
        }
    }

    void OnDrawGizmosSelected()
    {
        var oldColor = Gizmos.color;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Difficulty.Easy.landMineRange);
        Gizmos.color = oldColor;
    }
}
