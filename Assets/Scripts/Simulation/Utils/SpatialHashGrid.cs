using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct SpatialHashGrid : System.IDisposable
{
    private readonly float _cellSize;
    private NativeParallelMultiHashMap<int3, int> _grid;
    [ReadOnly] private int3 _gridSize; 

    public SpatialHashGrid(float cellSize, int initialCapacity, Allocator allocator, float3 bounds)
    {
        _cellSize = cellSize;
        _grid = new NativeParallelMultiHashMap<int3, int>(initialCapacity, allocator);
        _gridSize = (int3)math.ceil(bounds / _cellSize);
    }

    public void AddParticle(int index, float3 position)
    {
        int3 cell = GetCell(position);
        _grid.Add(cell, index);
    }

    public NativeList<int> GetNeighbors(float3 position, Allocator allocator)
    {
        NativeList<int> neighbors = new NativeList<int>(allocator);
        int3 cell = GetCell(position);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    int3 neighborCell = cell + new int3(x, y, z);
                    /*int3 neighborCell = new int3
                    {
                        x = cell.x + x % _gridSize.x,
                        y = cell.y + y % _gridSize.y,
                        z = cell.z + z % _gridSize.z,

                    };*/
                    if (_grid.TryGetFirstValue(neighborCell, out int neighborIndex, out NativeParallelMultiHashMapIterator<int3> iterator))
                    {
                        do
                        {
                            neighbors.Add(neighborIndex);
                        } while (_grid.TryGetNextValue(out neighborIndex, ref iterator));
                    }
                }
            }
        }

        return neighbors;
    }

    public void Clear()
    {
        _grid.Clear();
    }

    public void Dispose()
    {
        if (_grid.IsCreated)
        {
            _grid.Dispose();
        }
    }

    private int3 GetCell(float3 position)
    {
        return new int3(
            (int)math.floor(position.x / _cellSize),
            (int)math.floor(position.y / _cellSize),
            (int)math.floor(position.z / _cellSize)
        );
    }
}
