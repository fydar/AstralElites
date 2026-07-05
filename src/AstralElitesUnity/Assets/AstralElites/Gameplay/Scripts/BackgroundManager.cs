using System.Collections;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [Header("Scene")]
    public GameObjectPool<Character> playerPool;

    private void OnEnable()
    {
        AsteroidGenerator.instance.AsteroidPool.Flush();

        CreateCharacter();
    }

    private void CreateCharacter()
    {
        AsteroidGenerator.instance.Enable();
        var player = playerPool.Grab();

        player.Revive();
        int lastValue = player.Health.Value;
        player.Health.OnAfterChanged += () =>
        {
            if (player.Health.Value <= 0.0f)
            {
                if (player.isAlive)
                {
                    ScreenEffect.Instance.Pulse(1.0f);
                    Camera.main.GetComponent<PerlinShake>().PlayShake(1.0f);

                    AudioManager.Play(player.HitSoundAsset);
                    AudioManager.Play(player.DestroySoundAsset);

                    player.Kill();

                    AsteroidGenerator.instance.Disable();

                    StartCoroutine(GameOverRoutine());
                }
            }
            else
            {
                float delta = lastValue - player.Health.Value;
                lastValue = player.Health.Value;
                if (delta > 7)
                {
                    Camera.main.GetComponent<PerlinShake>().PlayShake(Mathf.InverseLerp(-30, 50, delta));
                }

                if (delta > 0)
                {
                    ScreenEffect.Instance.Pulse(delta / 60.0f);
                }
            }
        };

        player.GetComponent<CharacterAIController>().enabled = true;
    }

    private IEnumerator GameOverRoutine()
    {
        var waitForSecondsRealtime = new WaitForSecondsRealtime(0.1f);
        var pool = AsteroidGenerator.instance.AsteroidPool.pool;

        while (true)
        {
            bool poppedThisTick = false;

            // Iterate backwards to find the last active asteroid
            for (int i = pool.Count - 1; i >= 0; i--)
            {
                var asteroid = pool[i];
                if (asteroid != null && asteroid.isActiveAndEnabled)
                {
                    asteroid.Kill(asteroid.transform.position, asteroid.GetComponent<Rigidbody2D>().linearVelocity.normalized, false);
                    poppedThisTick = true;
                    break;
                }
            }

            // If we found and killed an asteroid, wait a tick before searching again
            if (poppedThisTick)
            {
                yield return waitForSecondsRealtime;
            }
            else
            {
                // No active asteroids were found in the entire pool, so we are done
                break;
            }
        }

        yield return waitForSecondsRealtime;

        playerPool.Flush();

        yield return waitForSecondsRealtime;

        CreateCharacter();
    }

    private void OnDisable()
    {
        AsteroidGenerator.instance.Disable();
    }
}
