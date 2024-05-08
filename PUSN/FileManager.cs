using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.Mathematics;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace PUSN
{
    public static class FileManager
    {
        static Vector3 prevPos;
        public static (List<Vector3> positions, int type, float radius) Load()
        {
            List<Vector3> positions = new List<Vector3>();   

            var currentDir = Directory.GetCurrentDirectory();

            var dialog = new OpenFileDialog() { InitialDirectory = Directory.GetParent(currentDir).Parent.Parent.FullName};

            if(dialog.ShowDialog() == false)
            {
                MessageBox.Show("Something went wrong.");
                //return false;
            }

            //var res = File.ReadAllText(dialog.FileName); 
            

           (int type, float radius) = ParseName(Path.GetFileName(dialog.FileName));

           var res = File.ReadLines(dialog.FileName);

            if(!res.Any())
            {
                MessageBox.Show("Could't load text file.");
                //return false;
            }

            foreach(var line in res )
            {
                Vector3 currentPos = ParseLine(line);
                positions.Add(currentPos);
                prevPos = currentPos;
            }

            return (positions,type,radius);
        }

        public static (int type, float r) ParseName(string name)
        {
            int IndDot = -1;
            for(int i=0; i<name.Length; i++)
            {
                if (name[i] == '.') IndDot = i;
            }
            if(IndDot == -1 ) { MessageBox.Show("Name of path file is incorrect! Dot is missing!"); }
            char t = name[IndDot + 1];
            string radius = name[(IndDot + 2)..];
            int type = -1;       // 0 - spherical, 1 - flat
            if (t == 'k') type = 0;
            else if (t == 'f') type = 1;
            else MessageBox.Show("Wrong type of milling tool!");

            float r =10;
            if (!float.TryParse(radius, out r)) MessageBox.Show("Couldn't parse radius!"); 

            return (type, r);
        }

        public static Vector3 ParseLine(string line)
        {
            int IndX=-1, IndY=-1, IndZ=-1;
            float xVal=float.MinValue, yVal= float.MinValue, zVal= float.MinValue;
            for(int i=0; i<line.Length; i++)
            {
                if (line[i]=='X') IndX = i;
                if (line[i]=='Y') IndY = i;
                if (line[i]=='Z') IndZ = i;               
            }

            // if all numbers are changed this is classic solution
            if(IndX != -1 && IndY!=-1)
            {
                string stringVal = line[(IndX+1) .. (IndY)];
                stringVal =stringVal.Replace('.', ',');

                if(!float.TryParse(stringVal, out xVal))
                {
                    MessageBox.Show("Couldn't parse X position!");
                }
            }

            if (IndY != -1 && IndZ != -1)
            {
                string stringVal = line[(IndY + 1)..(IndZ)];
                stringVal = stringVal.Replace('.', ',');
                if (!float.TryParse(stringVal, out yVal))
                {
                    MessageBox.Show("Couldn't parse Y position!");
                }
            }

            if(IndZ!=-1)
            {
                string stringVal = line[(IndZ+1)..];
                stringVal = stringVal.Replace('.', ',');
                if (!float.TryParse(stringVal, out zVal))
                {
                    MessageBox.Show("Couldn't parse Y position!");
                }
            }
            // end of classic solution

            // case: Only X changed
            if(IndX != -1 && IndY==-1 && IndZ==-1)
            {
                string stringVal = line[(IndX + 1)..];
                stringVal = stringVal.Replace('.', ',');
                if (!float.TryParse(stringVal, out xVal))
                {
                    MessageBox.Show("Couldn't parse X position!");
                }
            }

            // case: Only Y changed
            if (IndX == -1 && IndY != -1 && IndZ == -1)
            {
                string stringVal = line[(IndY + 1)..];
                stringVal = stringVal.Replace('.', ',');
                if (!float.TryParse(stringVal, out yVal))
                {
                    MessageBox.Show("Couldn't parse Y position!");
                }
            }

            // case: Only Z changed - Already handled correctly in classic solution
            // case: Only Y and Z changed - Already handled correctly in classic solution

            // case: Only X and Z changed - need to recover X value (Z already correct from classsic solution)
            if (IndX != -1 && IndY == -1 && IndZ != -1)
            {
                string stringVal = line[(IndX + 1)..(IndZ)];
                stringVal = stringVal.Replace('.', ',');
                if (!float.TryParse(stringVal, out xVal))
                {
                    MessageBox.Show("Couldn't parse X position!");
                }
            }
            // case: Only X and Y changed - need to recover Y value (X already correct from classsic solution)
            if (IndX != -1 && IndY != -1 && IndZ == -1)
            {
                string stringVal = line[(IndY + 1)..];
                stringVal = stringVal.Replace('.', ',');
                if (!float.TryParse(stringVal, out yVal))
                {
                    MessageBox.Show("Couldn't parse Y position!");
                }
            }

            if (xVal == float.MinValue) xVal = prevPos.X;
            if (yVal == float.MinValue) yVal = prevPos.Y;
            if (zVal == float.MinValue) zVal = prevPos.Z;
            return new Vector3(xVal, yVal, zVal);
        }
    }
}
