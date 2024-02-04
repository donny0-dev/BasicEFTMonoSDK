using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace BasicMonoSDK
{
    public class Loader
    {
        public static void Init()
        {
            GameObject SDKLoader = new GameObject();
            SDKLoader.AddComponent<Cheat>();
        }
    }
}
