using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Player {
  internal abstract class State {
    // -- properties
    internal StateName Name;
    internal int Frame;

    // -- lifetime
    internal State(StateName name) {
      Name = name;
      Frame = 0;
    }

    // -- commands
    internal void AdvanceFrame() {
      Frame++;
    }
  }
}
