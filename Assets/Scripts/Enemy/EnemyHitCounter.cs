using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitCounter : MonoBehaviour
{

    int _hitCount = 0;
    int _accumulatedScore = 0;
    bool _dieAfterGrounded = false;

    EnemyMobile _enemyMobile;

    private void Start()
    {
        _enemyMobile = GetComponent<EnemyMobile>();
    }

    private void Update()
    {
        if(_enemyMobile.Grounded && _dieAfterGrounded)
        {

            ScoreManager.instance.AddScore(_accumulatedScore);
            ScoreManager.instance.AmmoReward(_hitCount + 1);

            Destroy(gameObject);
        }
    }

    public void BeingHit()
    {
        if (!_enemyMobile.Grounded)
        {
            _hitCount++;

            if (_hitCount >= 1 && _hitCount <= 3)
                _accumulatedScore += 150;
            else if (_hitCount > 3)
                _accumulatedScore += 450;
            else
                _accumulatedScore += 150;

            int heightScoreWhenHit = Mathf.FloorToInt(transform.position.y - Terrain.activeTerrain.SampleHeight(transform.position)) * 10;
            _accumulatedScore += heightScoreWhenHit;

            _dieAfterGrounded = true;
        }
    }

}
