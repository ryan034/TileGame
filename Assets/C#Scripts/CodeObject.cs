using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class CodeObject
{
    private Dictionary<string, string> variablesSingle = new Dictionary<string, string>();
    private List<KeyValuePair<string, CodeObject>> functionsList = new List<KeyValuePair<string, CodeObject>>();
    private Dictionary<string, List<string>> variablesList = new Dictionary<string, List<string>>();

    // all variablesSingle contribute to control of codeobject: 
    // if codeobject has no functionslist then it is a simple function,
    // if codeobject has functionlist and no variablesSingle it is simply a collection of codeobjects to be parsed sequentially
    // if codeobject has both variablesSingle and functionslist, then variablesSingle is the control structure and functionslist is to be parsed sequentially based on the control structure
    // all codeobjects has access to array of int,vector,unit variables supplied by stackitem

    public string Task { get; private set; }//task name: can be attack,spell,spawn,conditional function, loop function, while function, OnAttack filter, Animate, 

    public static CodeObject LoadCode(string xml, XElement element = null)
    {
        element = string.IsNullOrEmpty(xml) ? element : XDocument.Parse(xml).Root;
        if (element == null) return null;
        CodeObject result = new CodeObject();
        element.Elements().ToList().ForEach(i =>
        {
            // if it's part of a collection then add the collection items to a temp variable 
            if (i.HasElements)
            {
                if (i.Elements().Count() > 1 && i.Elements().All(e => e.Name.LocalName == i.Elements().First().Name.LocalName) || i.Name.LocalName == Pluralize(i.Elements().First().Name.LocalName))
                {
                    i.Elements().ToList().ForEach(x => { result.variablesList[i.Name.LocalName].Add(x.Value); });
                }
                else result.functionsList.Add(new KeyValuePair<string, CodeObject>(i.Name.LocalName, LoadCode(null, i))); //if it's not a collection, but it has child elements then it's either a complex property or a simple property create a property with the current child elements name and process its properties
            }
            else
            {
                if (i.Name.LocalName != "task")
                {
                    //Debug.Log(i.Name.LocalName);
                    //Debug.Log(i.Value);
                    result.variablesSingle[i.Name.LocalName] = i.Value; // create a property and just add the value
                }
                else
                {
                    result.Task = i.Value;
                }
            }
        });
        return result;
    }

    public bool IsConditional
    {
        get
        {
            foreach (KeyValuePair<string, CodeObject> s in functionsList)
            {
                if(s.Key== "true" || s.Key == "false") { return true; }
            }
            return false;
        }
    }

    public string GetVariable(string s) { if (variablesSingle.ContainsKey(s)) { return variablesSingle[s]; } else { return ""; } }

    public IEnumerable<string> GetListVariables(string s)
    {
        if (variablesList.ContainsKey(s))
        {
            foreach (string str in variablesList[s])
            {
                yield return str;
            }
        }
    }

    public IEnumerable<CodeObject> GetCodeObjects(string s)
    {
        foreach (KeyValuePair<string, CodeObject> f in functionsList)
        {
            if (f.Key == s) { yield return f.Value; }
        }
    }

    private static string Pluralize(string localName)
    {
        return localName + "s";
    }
    /*
    private dynamic GetAnonymousType(string xml, XElement element = null)
    {
        // either set the element directly or parse XML from the xml parameter.
        element = string.IsNullOrEmpty(xml) ? element : XDocument.Parse(xml).Root;
        // if there's no element than there's no point to continue
        if (element == null) return null;
        IDictionary<string, dynamic> result = new ExpandoObject();
        // grab any attributes and add as properties
        element.Attributes().AsParallel().ForAll(attribute => result[attribute.Name.LocalName] = attribute.Value);
        // check if there are any child elements.
        if (!element.HasElements)
        {
            // check if the current element has some value and add it as a property
            if (!string.IsNullOrWhiteSpace(element.Value)) result[element.Name.LocalName] = element.Value;
            return result;
        }
        // Check if the child elements are part of a collection (array). If they are not then
        // they are either a property of complex type or a property with simple type
        var isCollection = (element.Elements().Count() > 1 && element.Elements().All(e => e.Name.LocalName == element.Elements().First().Name.LocalName) || element.Name.LocalName == Pluralize(element.Elements().First().Name.LocalName));
        var values = new ConcurrentBag<dynamic>();
        // check each child element
        element.Elements().ToList().AsParallel().ForAll(i =>
        {
            // if it's part of a collection then add the collection items to a temp variable 
            if (isCollection) values.Add(GetAnonymousType(null, i));
            else if (i.HasElements) result[i.Name.LocalName] = GetAnonymousType(null, i); //if it's not a collection, but it has child elements then it's either a complex property or a simple property create a property with the current child elements name and process its properties
            else result[i.Name.LocalName] = i.Value; // create a property and just add the value
        });
        // for collection items we want skip creating a property with the child item names, 
        // but directly add the child properties to the
        if (values.Count > 0) result[element.Name.LocalName] = values;
        // return the properties of the processed element
        return result;
    }*/

}