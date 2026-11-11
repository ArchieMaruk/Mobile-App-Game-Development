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

    [SerializeField] int testChunk = -1;   // set 0-9 to only spawn that chunk, -1 = random
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
                float length = SpawnChunk();
                float gap = Mathf.Lerp(startGap, endGap, GameManager.Instance.Progress);
                float wait = gap + length / GameManager.Instance.Speed;   // extra time for long chunks
                await Awaitable.WaitForSecondsAsync(wait, ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    // Spawns one chunk and returns how long it is in meters,
    // so the next chunk doesn't overlap it.
    float SpawnChunk()
    {
        float length = 0f;
        int lane = UnityEngine.Random.Range(0, 3);
        int type = testChunk >= 0 ? testChunk : UnityEngine.Random.Range(0, 10);

        switch (type)
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

            case 4: // pickup line
                for (int i = 0; i < 5; i++) pickups.Spawn(Lane(lane, 1f, i * 2f));
                length = 8f;
                break;

            case 5: // knife, then bottles: jump, then get to the free lane
                lowBarriers.Spawn(new Vector3(0f, 0.25f, spawnZ));
                for (int i = 0; i < 3; i++)
                    if (i != lane) blocks.Spawn(Lane(i, 1f, 8f));
                length = 8f;
                break;

            case 6: // dish rack, then a bottle: slide, then dodge
                highBarriers.Spawn(new Vector3(0f, 2.6f, spawnZ));
                blocks.Spawn(Lane(lane, 1f, 8f));
                length = 8f;
                break;

            case 7: // zigzag: free lane switches side each row
                for (int row = 0; row < 3; row++)
                {
                    int free = row % 2 == 0 ? 0 : 2;
                    for (int i = 0; i < 3; i++)
                        if (i != free) blocks.Spawn(Lane(i, 1f, row * 8f));
                }
                length = 16f;
                break;

            case 8: // coin arc over a knife: jump to collect
                lowBarriers.Spawn(new Vector3(0f, 0.25f, spawnZ));
                for (int i = 0; i < 5; i++)
                {
                    float z = (i - 2) * 2f;            // -4, -2, 0, 2, 4
                    float y = 1f + 1.5f * (1f - (z / 4f) * (z / 4f));
                    pickups.Spawn(Lane(lane, y, z));
                }
                length = 4f;
                break;

            default: // 9: bottle wall with coins in the gap
                for (int i = 0; i < 3; i++)
                    if (i != lane)
                    {
                        blocks.Spawn(Lane(i, 1f));
                        blocks.Spawn(Lane(i, 1f, 3f));
                    }
                for (int i = 0; i < 3; i++) pickups.Spawn(Lane(lane, 1f, i * 2f));
                length = 4f;
                break;
        }

        return length;
    }

    Vector3 Lane(int lane, float y, float extraZ = 0f) =>
        new Vector3((lane - 1) * laneWidth, y, spawnZ + extraZ);
}