using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _2048
{
    [CreateAssetMenu(menuName ="2048/TileState")]
    public class TileState : ScriptableObject
    {
        public Color backgroundColor = Color.black;
        public Color textColor = Color.white;
    }
}