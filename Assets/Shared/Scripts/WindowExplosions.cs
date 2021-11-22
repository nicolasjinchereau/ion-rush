using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowExplosions : MonoBehaviour
{
    public List<GameObject> spawnAreas;
    public GameObject explosionPrefab;
    public float minExplosionInterval = 9.0f;
    public float maxExplosionInterval = 20.0f;
    public AudioSource explosionSound;

    void OnEnable()
    {
        StartCoroutine(DoExplosion());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator DoExplosion()
    {
        var delay = Random.Range(minExplosionInterval, maxExplosionInterval);

        yield return new WaitForSeconds(delay);

        GameObject area = null;
        float minDist = float.MaxValue;

        foreach (var spawn in spawnAreas)
        {
            var dist = Vector3.Distance(Player.position, spawn.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                area = spawn;
            }
        }

        var areaScaleX = area.transform.localScale.x;
        var pos = area.transform.position;
        pos.x = pos.x + Random.Range(-areaScaleX, areaScaleX) * 0.5f;
        
        var go = Instantiate(explosionPrefab, pos, Quaternion.identity);
        CameraController.that.DoImpactShake();
        explosionSound.ignoreListenerVolume = true;
        explosionSound.Play();

        yield return new WaitForSeconds(3.0f);
        
        Destroy(go);

        StartCoroutine(DoExplosion());
    }
}
