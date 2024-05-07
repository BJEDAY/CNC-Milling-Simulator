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
        public static bool Load()
        {
            List<Vector3> positions = new List<Vector3>();   

            var currentDir = Directory.GetCurrentDirectory();

            var dialog = new OpenFileDialog() { InitialDirectory = currentDir};

            if(dialog.ShowDialog() == false)
            {
                MessageBox.Show("Something went wrong.");
                return false;
            }

            //var res = File.ReadAllText(dialog.FileName);

           var res = File.ReadLines(dialog.FileName);

            if(!res.Any())
            {
                MessageBox.Show("Could't load text file.");
                return false;
            }

            foreach(var line in res )
            {
                positions.Add(ParseLine(line));
            }

            return true;
        }

        public static Vector3 ParseLine(string line)
        {
            int IndX=-1, IndY=-1, IndZ=-1;
            float xVal=0, yVal=0, zVal=0;
            for(int i=0; i<line.Length; i++)
            {
                if (line[i]=='X') IndX = i;
                if (line[i]=='Y') IndY = i;
                if (line[i]=='Z') IndZ = i;               
            }
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
            
            return new Vector3(xVal, yVal, zVal);
        }
    }
}
