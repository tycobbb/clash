using System;

namespace Clash.Input {
  /// A queue of the last n Snapshots.
  ///
  /// If less than n frames have elapsed, the unfilled values will simply return
  /// the default Snapshot. This is bad data, but it should not be a problem in
  /// practice.
  public sealed class Buffer {
    // -- properties --
    private readonly Snapshot[] queue;
    private int head = -1;

    // -- lifetime --
    public Buffer(uint size) {
      queue = new Snapshot[size];
    }

    // -- commands --
    /// Adds a new snapshot to the buffer, removing the oldest one.
    public void Add(Snapshot snapshot) {
      head = GetIndex(-1);
      queue[head] = snapshot;
    }

    // -- queries --
    /// Gets the snapshot nth-newest snapshot.
    public Snapshot this[uint offset] {
      get {
        if (offset >= queue.Length) {
          throw new IndexOutOfRangeException();
        } else {
          return queue[GetIndex((int)offset)];
        }
      }
    }

    /// Gets the circular index given an from the start index.
    private int GetIndex(int offset) {
      return ((head - offset) + queue.Length) % queue.Length;
    }
  }
}
