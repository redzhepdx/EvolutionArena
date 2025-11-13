using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionUtils{
    public static class Utils{
        public static Vector3 index2position(uint index, float offset, uint layer_size, uint row_size){
            // Get real position
            uint z = (uint)Mathf.Floor(index / layer_size);
            uint y = (uint)Mathf.Floor((index % layer_size) / row_size);
            uint x = index - (z * layer_size + y * row_size);

            // Create real world coordiante
            return new Vector3((float)x * offset, (float)y * offset, (float)z * offset);
        }
    }
}
