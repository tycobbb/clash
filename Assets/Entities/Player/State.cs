using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Player {
  public abstract class State {
    // -- properties
    public StateName Name;
    public int Frame;

    // -- lifetime
    public State(StateName name) {
      Name = name;
      Frame = 0;
    }

    // -- commands
    public void AdvanceFrame() {
      Frame++;
    }
  }
}
