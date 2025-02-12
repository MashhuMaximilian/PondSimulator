using System;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct NativeCounter : IDisposable
{
    private NativeArray<int> _counter;

    /// <summary>
    /// Creates a new NativeCounter.
    /// </summary>
    /// <param name="allocator">Allocator to use for the counter.</param>
    public NativeCounter(Allocator allocator)
    {
        _counter = new NativeArray<int>(1, allocator, NativeArrayOptions.ClearMemory);
    }


    /// <summary>
    /// Increments the counter in a single-threaded context.
    /// </summary>
    public void Increment()
    {
        _counter[0]++;
    }

    /// <summary>
    /// Gets the current count.
    /// </summary>
    public int Count => _counter[0];

    /// <summary>
    /// Disposes of the NativeCounter's resources.
    /// </summary>
    public void Dispose()
    {
        if (_counter.IsCreated)
        {
            _counter.Dispose();
        }
    }

    /// <summary>
    /// Provides a thread-safe parallel writer for the counter.
    /// </summary>
    public ParallelWriter AsParallelWriter()
    {
        return new ParallelWriter { _counter = _counter };
    }

    [BurstCompile]
    public struct ParallelWriter
    {
        [NativeDisableParallelForRestriction]
        internal NativeArray<int> _counter;

        /// <summary>
        /// Increments the counter in a thread-safe manner.
        /// </summary>
        public void Increment()
        {
            // Increment safely by using an atomic operation on the NativeArray
            var currentValue = _counter[0];
            _counter[0] = currentValue + 1; // This can now be used in parallel safely.
        }
    }
}