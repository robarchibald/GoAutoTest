﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GoAutoTest
{
  public class ProcessOutput
  {
    public string[] StandardError { get; set; }
    public string[] StandardOutput { get; set; }
  }
}