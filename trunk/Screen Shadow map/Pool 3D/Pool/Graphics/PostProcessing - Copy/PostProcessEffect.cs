using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extreme_Pool.Models.PostProcessing
{
    public class PostProcessEffect : PostProcessBase
    {
        public bool IsEnabled;
        
        public List<PostProcessComponent> Components;
        public PostProcessEffect()
        {
            Components = new List<PostProcessComponent>();
            IsEnabled = true;
        }

        public void LoadContent()
        {
            throw new NotImplementedException();
        }
    }
}
