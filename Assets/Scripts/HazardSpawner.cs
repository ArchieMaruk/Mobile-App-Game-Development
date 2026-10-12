using System;
using System.Threading;
using UnityEngine;

public class HazardSpawner : MonoBehaviour
{
    [SerializeField] HazardPool blocks, lowBarriers, highBarriers, pickups;
    [SerializeField] float laneWidth = 2f;
    [SerializeField] float spawnZ = 40f;
    [SerializeField] float startGap = 1.6f;
    [SerializeField] float endGap = 0.8f;
    CancellationTokenSource cts;

    void OnEnable()
    {
        cts = CancellationTokenSource.CreateLinkedTokenSource(
            Application.exitCancellationToken);
        _ = RunAsync(cts.Token);
    }

    void OnDisable() { cts.Cancel(); cts.Dispose(); }

    async Awaitable RunAsync(CancellationToken ct)
    {
        try
        {
            await Awaitable.NextFrameAsync(ct);   // let GameManager start first
            while (true)
            {
                SpawnChunk();
                float gap = Mathf.Lerp(startGap, endGap, GameManager.Instance.Progress);
                await Awaitable.WaitForSecondsAsync(gap, ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    void SpawnChunk()
    {
        int lane = UnityEngine.Random.Range(0, 3);
        switch (UnityEngine.Random.Range(0, 5))
        {
            case 0: // single block
                blocks.Spawn(Lane(lane, 1f));
                break;
            case 1: // double block, one lane free
                for (int i = 0; i < 3; i++)
                    if (i != lane) blocks.Spawn(Lane(i, 1f));
                break;
            case 2: // low barrier: jump
                lowBarriers.Spawn(new Vector3(0f, 0.25f, spawnZ));
                break;
            case 3: // high barrier: slide
                highBarriers.Spawn(new Vector3(0f, 2.6f, spawnZ));
                break;
            default: // pickup line
                for (int i = 0; i < 5; i++) pickups.Spawn(Lane(lane, 1f, i * 2f));
                break;
        }
    }

    Vector3 Lane(int lane, float y, float extraZ = 0f) =>
        new Vector3((lane - 1) * laneWidth, y, spawnZ + extraZ);
}