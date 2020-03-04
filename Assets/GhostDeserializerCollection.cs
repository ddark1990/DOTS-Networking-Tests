using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using Unity.NetCode;

public struct NetCubeGhostDeserializerCollection : IGhostDeserializerCollection
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public string[] CreateSerializerNameList()
    {
        var arr = new string[]
        {
            "NetCubeGhostSerializer",
        };
        return arr;
    }

    public int Length => 1;
#endif
    public void Initialize(World world)
    {
        var curNetCubeGhostSpawnSystem = world.GetOrCreateSystem<NetCubeGhostSpawnSystem>();
        m_NetCubeSnapshotDataNewGhostIds = curNetCubeGhostSpawnSystem.NewGhostIds;
        m_NetCubeSnapshotDataNewGhosts = curNetCubeGhostSpawnSystem.NewGhosts;
        curNetCubeGhostSpawnSystem.GhostType = 0;
    }

    public void BeginDeserialize(JobComponentSystem system)
    {
        m_NetCubeSnapshotDataFromEntity = system.GetBufferFromEntity<NetCubeSnapshotData>();
    }
    public bool Deserialize(int serializer, Entity entity, uint snapshot, uint baseline, uint baseline2, uint baseline3,
        ref DataStreamReader reader, NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                return GhostReceiveSystem<NetCubeGhostDeserializerCollection>.InvokeDeserialize(m_NetCubeSnapshotDataFromEntity, entity, snapshot, baseline, baseline2,
                baseline3, ref reader, compressionModel);
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }
    public void Spawn(int serializer, int ghostId, uint snapshot, ref DataStreamReader reader,
        NetworkCompressionModel compressionModel)
    {
        switch (serializer)
        {
            case 0:
                m_NetCubeSnapshotDataNewGhostIds.Add(ghostId);
                m_NetCubeSnapshotDataNewGhosts.Add(GhostReceiveSystem<NetCubeGhostDeserializerCollection>.InvokeSpawn<NetCubeSnapshotData>(snapshot, ref reader, compressionModel));
                break;
            default:
                throw new ArgumentException("Invalid serializer type");
        }
    }

    private BufferFromEntity<NetCubeSnapshotData> m_NetCubeSnapshotDataFromEntity;
    private NativeList<int> m_NetCubeSnapshotDataNewGhostIds;
    private NativeList<NetCubeSnapshotData> m_NetCubeSnapshotDataNewGhosts;
}
public struct EnableNetCubeGhostReceiveSystemComponent : IComponentData
{}
public class NetCubeGhostReceiveSystem : GhostReceiveSystem<NetCubeGhostDeserializerCollection>
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireSingletonForUpdate<EnableNetCubeGhostReceiveSystemComponent>();
    }
}
