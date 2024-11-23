public class NetworkManager : MonoBehaviour
{
    private float timer;
    public static int tick;

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = NetworkSettings.tickrate;

        LagCompensation.Start(Server.MaxPlayers);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= NetworkSettings.tickTime)
        {
            timer -= NetworkSettings.tickTime;

            ProcessTick();
            tick++;
        }
    }

    private void ProcessTick()
    {
        ThreadManager.UpdateMain();

        // Update player positions here.

        LagCompensation.SavePlayerStates();

        // ... such as sending snapshots of the game to the clients 
    }

    public void RollbackPlayer(Vector3 position, Quaternion rotation, int playerId, int targetPlayerId)
    {
        Hitbox hitbox = Instantiate(Server.clients[playerId].player.hitboxPrefab, position, rotation)
            .GetComponent<Hitbox>();
        hitbox.Initialize(playerId, targetPlayerId);
    }

    private void OnApplicationQuit()
    {
        LagCompensation.Stop();
    }
}