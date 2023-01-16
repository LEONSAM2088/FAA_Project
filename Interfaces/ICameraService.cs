using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAA_Project { 
    public interface ICameraService
    {
        public string Emotion { get; set; }
        public void OpenCamera(int width, int height);
        public void OnClick();
    }
}
