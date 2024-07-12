using System.Collections;
using UnityEngine;

public interface Logger
{
    public void log(string str);
    public string [] connectorAccess(); // returns everything that goes into the UI, should be available to each app
    // return x, y of eyes, head position (see Kevin's), timer variables, convergence calculation, 

    public void storePath(string [] ReaderValues, string ReaderNames); // provides two arrays one with data the other with the name of the data for logging header pourpuses

    public string[] connectorFieldNames(); //provides the names of the daqta being accesed used fro grapher names
    
    public string[] outputValues();

}