using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace PickClone
{
    class StepViewModel
    {
        ObservableCollection<StepDesc> stepDescs = new ObservableCollection<StepDesc>();
        public StepViewModel()
        {
            string sIconFolder = FolderHelper.GetIconFolder();
            BitmapImage calib = new BitmapImage(new Uri(sIconFolder + "Calibration.jpg"));
            BitmapImage acquire = new BitmapImage(new Uri(sIconFolder + "Camera.png"));
            
            stepDescs.Add(new StepDesc("采集图像", acquire,Stage.Acquire));
            stepDescs.Add(new StepDesc("设置条件", calib,Stage.Setting));
        }

        public ObservableCollection<StepDesc> StepsModel
        {
            get
            {
                return stepDescs;
            }
            set
            {
                stepDescs = value;
            }
        }
    }


    class StepDesc
    {
        string name;
        Stage correspondingStage;
        BitmapImage image;

        public StepDesc(string name, BitmapImage bmp, Stage stage)
        {
            this.name = name;
            this.image = bmp;
            correspondingStage = stage;
        }
        public Stage CorrespondingStage
        {
            get { return correspondingStage; }
            set { correspondingStage = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public BitmapImage Image
        {
            get { return image; }
            set { image = value; }
        }
        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            StepDesc anotherDesc = obj as StepDesc;
            if ((System.Object)anotherDesc == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (name == anotherDesc.name);
        }

        public static bool operator ==(StepDesc a, StepDesc b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(StepDesc a, StepDesc b)
        {
            return !(a == b);
        }

    }
}
