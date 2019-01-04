﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Экспертная_система
{
    [Serializable]
    public class Hyperparameters
    {
        public int newNodeIdWillBe = 0;
        public Hyperparameters(Form1 form1)
        {
            this.form1 = form1;
            addByParentId(-1, "name:baseNode");
        }

        [NonSerializedAttribute]
        public Form1 form1;
        public PictureBox picBox;
        public Graphics g;
        public Bitmap bitmap;

        public List<Node> nodes = new List<Node>();
       public int Y=0;
        ////////////////// ADD //////////////////////////////
        public void add(int parentID, string name, string category, string categories)
        {
            addByParentId(parentID, "name:" + name + ",category:" + category + ",categories:" + categories
                + ",is_categorical:true,is_change:false,is_change_up_or_down:false,is_const:false");
        }
        public void add(string name, string category, string categories)
        {
            add("name:" + name + ",value:" + category + ",categories:" + categories
                + ",is_categorical:true,is_change:false,is_change_up_or_down:false,is_const:false");
        }

        public int add(string attributes)
        {
            if (!attributes.Contains("name"))
            {  //если в записи атрибутов нет атрибута-имени (например "pathPrefix:D:\\Anton\\Desktop\\MAIN")
                //добавляется атрибут-имя соответсвующее имени единственного атрибута, а его имя заменяется на "valuez"
                attributes = "name:" + attributes.Split(':')[0] + ',' + attributes.Replace(attributes.Split(':')[0], "value");
            }
            Node newNode = new Node(newNodeIdWillBe, 0, attributes);
            nodes.Add(newNode);
            newNodeIdWillBe++;
            return newNodeIdWillBe - 1;
        }

        public int addByParentId(int parentID, string attributes)
        {
            if (!attributes.Contains("name"))
            {  //если в записи атрибутов нет атрибута-имени (например "pathPrefix:D:\\Anton\\Desktop\\MAIN")
                //добавляется атрибут-имя соответсвующее имени единственного атрибута, а его имя заменяется на "valuez"
                attributes = "name:" + attributes.Split(':')[0] + ',' + attributes.Replace(attributes.Split(':')[0], "value");
            }
            Node newNode = new Node(newNodeIdWillBe, parentID, attributes);
            nodes.Add(newNode);
            newNodeIdWillBe++;
            return newNodeIdWillBe - 1;
        }
        public int addLeafByParentId(int parentID, string attributes)
        {
            if (!attributes.Contains("name"))
            {  //если в записи атрибутов нет атрибута-имени (например "pathPrefix:D:\\Anton\\Desktop\\MAIN")
                //добавляется атрибут-имя соответсвующее имени единственного атрибута, а его имя заменяется на "valuez"
                attributes = "name:" + attributes.Split(':')[0] + ',' + attributes.Replace(attributes.Split(':')[0], "value");
            }
            Node newNode = new Node(newNodeIdWillBe, parentID, attributes);
            newNode.isLeaf = true;
            nodes.Add(newNode);
            newNodeIdWillBe++;
            return newNodeIdWillBe - 1;
        }
        //CONST
        public void add(string name, int value)
        {
            add("name:" + name + ",value:" + value
               + ",is_categorical:false,is_change:false,is_change_up_or_down:false,is_const:true");
        }
        public void add(string name, string value)
        {
            add("name:" + name + ",value:" + value
               + ",is_categorical:false,is_change:false,is_change_up_or_down:false,is_const:true");
        }
        public void add(string name, int value, int min, int max)
        {
            add("name:" + name + ",value:" + value
           + "min:" + min + "max:" + max + ", is_categorical:false,is_change:false,is_change_up_or_down:false,is_const:false");

        }
        ////////////////// GET //////////////////////////////
        public Node getNodeById(int ID)
        {
            if (nodes[ID].ID == ID)
            {
                return nodes[ID];
            }
            else
            {
                log("не найден параметр по ID = " + ID.ToString(), System.Drawing.Color.Red);
                return null;
            }
        }

        private int lastIdBySameParentId = 0;
        public List<Node> getNodesByparentID(int parentID)
        {
            List<Node> resnodes = new List<Node>();
            /*   if (lastIdBySameParentId + 1 < nodes.Count)
               {
                   if (getNodeById(lastIdBySameParentId + 1).parentID == parentID)
                   {
                       lastIdBySameParentId++;
                       resnodes.Add(getNodeById(lastIdBySameParentId + 1));
                   }
               }  */
            if (nodes[parentID].isLeaf)
            {
                return resnodes;
            }
            int count = nodes.Count;
            if (nodes[parentID].getAttributeValue("count") != null)
            {
                int targetCount = Convert.ToInt32(nodes[parentID].getAttributeValue("count"));
                int inc = 0;
                for (int i = 0; i < count; i++)
                {
                    if (nodes[i].parentID == parentID)
                    {
                        lastIdBySameParentId = nodes[i].ID;
                        resnodes.Add(nodes[i]);
                        inc++;
                        if (inc == targetCount)
                            break;
                    }
                }
                return resnodes;

            }
            for (int i = 0; i < count; i++)
            {
                if (nodes[i].parentID == parentID)
                {
                    lastIdBySameParentId = nodes[i].ID;
                    resnodes.Add(nodes[i]);
                }
            }
            return resnodes;
        }
        public string getValueByName(string name)
        {

            Node node = null;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].getAttributeValue("name") == name)
                    node = nodes[i];
            if (node == null)
            {
                log("аттрибут " + name + " не найден", System.Drawing.Color.Red);
                return "аттрибут " + name + " не найден";
            }
            return node.getAttributeValue("value");
        }
        public List<Node> getNodeByName(string name)
        {
            List<Node> nodes = new List<Node>();
            Node node = null;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].getAttributeValue("name") == name)
                    nodes.Add(nodes[i]);
            if (node == null)
            {
                log("узел " + name + " не найден", System.Drawing.Color.Red);
                return null;
            }
            return nodes;
        }

        ////////////////// SET //////////////////////////////
        public void setAttributeByName(string name, int value)
        {
            getNodeByName("name")[0].setAttribute(name, value.ToString());
        }
        public void setAttributeByName(string name, string value)
        {
            getNodeByName("name")[0].setAttribute(name, value);
        }

        ////////////////// VARIATE //////////////////////////////
        public void variate(string name)
        {
            Node node = getNodeByName(name)[0];
            Random r = new Random();
            if (!Convert.ToBoolean(node.getAttributeValue("is_const")))
                if (Convert.ToBoolean(node.getAttributeValue("is_categorical")))
                {
                    if (Convert.ToBoolean(node.getAttributeValue("categories") != null))
                    {
                        string lastValue = node.getValue();
                        node.setAttribute("value", node.getAttributeValue("categories").Split('|')[r.Next(node.getAttributeValue("categories").Split('|').Length)]);
                        if (lastValue == node.getValue())
                        {
                            node.setAttribute("is_change", "true");
                        }
                        else
                        {
                            node.setAttribute("is_change", "false");
                        }
                    }
                    else
                    {
                        log("не удалось варьировать категориальный параметр: множество категорий пустое", System.Drawing.Color.Red);
                    }
                }
                else
                {
                    int last_value = Convert.ToInt32(node.getValue());
                    int new_value = 0;

                    new_value = r.Next(Convert.ToInt32(node.getAttributeValue("min")), Convert.ToInt32(node.getAttributeValue("max")));

                    if (new_value > last_value)
                        node.setAttribute("is_change_up_or_down", "true");

                    if (new_value < last_value)
                        node.setAttribute("is_change_up_or_down", "false");

                    if (last_value != new_value)
                    {
                        node.setAttribute("is_change", "true");
                    }
                    else
                    {
                        node.setAttribute("is_change", "false");
                    }
                }
        }


        public void draw(int rootId, PictureBox target_pictureBox, Form1 form1, int h, int columnWidth)
        {
            bool isFirstTime = true;
        drawHyperparametersAgain:

            totalAttributesNumber = 0;
            deepnessRate = 0;
            this.h = h;
            currentH = 0;
            mainDepth = Convert.ToInt16(h * 0.65);
            this.columnWidth = columnWidth;
            this.form1 = form1;
            incGetNodeByID = 0;
            totalAttributesNumber = recurciveAttributeCountSearch(rootId);
            picBox = target_pictureBox;
            // picBox.Height = h * totalAttributesNumber+400;

            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
            recurciveAttributeDRAW(rootId);
            refresh();
            if (isFirstTime)
            {
                isFirstTime = false;
                goto drawHyperparametersAgain;
            }


        }
        public string toJSON(int ID)
        {
            // string jsonText = JsonConvert.SerializeObject(this);
            string jsonText = buildJSON(ID);
            return '{' + jsonText + '}';
        }

        private List<string> names = new List<string>();
        private List<string> namesWhithIndex = new List<string>();
        private string buildJSON(int ID)
        {
            string s = "";

            var childrens = getNodesByparentID(ID);
            if (childrens.Count == 0)
            {
                Node node = getNodeById(ID);

                string nodeName = node.getAttributeValue("name");

                if (names.Contains(nodeName))
                {
                    nodeName = nodeName + "(2)";
                    for (int i = 0; i < namesWhithIndex.Count; i++)
                        if (namesWhithIndex[i].Split('(', ')')[0] == nodeName.Split('(', ')')[0])
                        {
                            string lastName = namesWhithIndex[i];
                            namesWhithIndex.Remove(nodeName);
                            nodeName = nodeName.Split('(', ')')[0] + '(' + (Convert.ToInt16(lastName.Split('(', ')')[1]) + 1).ToString() + ')';
                        }
                    namesWhithIndex.Add(nodeName);
                }
                else
                    names.Add(nodeName);
                s += '"' + nodeName + '"' + ":{";
                foreach (Attribute attr in node.attributes)
                {
                    if (attr.name != "name")
                        s += '"' + attr.name + '"' + ':' + '"' + attr.value + '"' + ',';
                }
                s = s.Remove(s.Length - 1, 1);
                s += '}';
                return s;
            }
            else
            {
                Node node = getNodeById(ID);
                if (node.getAttributeValue("name") == null)
                {
                    s += '"' + node.attributes[0].value + '"' + ":{";
                }
                else
                {

                    string nodeName = node.getAttributeValue("name");

                    if (names.Contains(nodeName))
                    {
                        nodeName = nodeName + "(2)";
                        for (int i = 0; i < namesWhithIndex.Count; i++)
                            if (namesWhithIndex[i].Split('(', ')')[0] == nodeName.Split('(', ')')[0])
                            {
                                string lastName = namesWhithIndex[i];
                                namesWhithIndex.Remove(nodeName);
                                nodeName = nodeName.Split('(', ')')[0] + '(' + (Convert.ToInt16(lastName.Split('(', ')')[1]) + 1).ToString() + ')';
                            }
                        namesWhithIndex.Add(nodeName);
                    }
                    else
                        names.Add(nodeName);
                    s += '"' + nodeName + '"' + ":{";
                }
                if (node.attributes != null)
                {
                    foreach (Attribute attr in node.attributes)
                    {
                        if (attr.name != "name")
                            s += '"' + attr.name + '"' + ':' + '"' + attr.value + '"' + ',';
                    }
                }
                names.Clear();
                namesWhithIndex.Clear();
                for (int i = 0; i < childrens.Count; i++)
                {
                    s += buildJSON(childrens[i].ID);
                    if (i != childrens.Count - 1)
                        s += ',';

                }

                s += '}';

            }

            return s;
        }
        public Node fromJSON(string JSON, int parentId)
        {
            JSON = JSON.Replace("\"", "");
            int endOfName = JSON.IndexOf(':');
            string name = JSON.Substring(0, endOfName).Replace("{", "");

            Node newNode = new Node(newNodeIdWillBe, parentId, "name:" + name);
            JSON = JSON.Remove(0, endOfName + 2);
            int endOfAttr = JSON.IndexOf('{');
            if (endOfAttr == -1)
                endOfAttr = JSON.Length - 1;
            string[] RAWattr = JSON.Substring(0, endOfAttr).Split(',');
            for (int i = 0; i < RAWattr.Length; i++)
            {
                if (RAWattr[i].Split(':')[1] != "")
                {
                    if (RAWattr[i][RAWattr[i].Length - 1] == '}')
                    {
                        newNode.addAttribute(RAWattr[i].Substring(0, RAWattr[i].Length - 1));
                    }
                    else
                    {
                        newNode.addAttribute(RAWattr[i]);
                    }
                }
            }
            nodes.Add(newNode);
            newNodeIdWillBe++;
            int rate = 1;
            //поиск ветвей
            List<string> branches = new List<string>();
            int beginOfChildrenText = 0;
            int endOfChildrenText = 0;
            int endOfPreviousBranch = JSON.IndexOf(RAWattr[RAWattr.Length - 1]);
            for (int i = 0; i < JSON.Length; i++)
            {
                if (JSON[i] == '{')
                {
                    rate++;
                    if (rate == 2)
                    {
                        beginOfChildrenText = i;
                    }
                }
                if (JSON[i] == '}')
                {
                    rate--;
                    if (rate == 1)
                    {
                        endOfChildrenText = i;
                        branches.Add(JSON.Substring(endOfPreviousBranch, endOfChildrenText - endOfPreviousBranch + 1));
                        endOfPreviousBranch = i + 2;
                    }
                }
            }
            //цикл по дочерним нодам
            for (int i = 0; i < branches.Count; i++)
            {
                fromJSON(branches[i], newNode.ID);
            }

            return newNode;
        }


        private int totalAttributesNumber = 0;
        public int deepnessRate = 0;
        public int h = 15;
        public int currentH = 0;
        public int mainDepth = 20;
        public int columnWidth = 150;
        private void recurciveAttributeDRAW(int ID)
        {

            var childrens = getNodesByparentID(ID);
            if (childrens.Count == 0)
            {

                //горизонтальные линии листочков
                drawLine(Color.WhiteSmoke, 1, (columnWidth) * deepnessRate, currentH * h, (columnWidth) * (deepnessRate + 1), currentH * h);
                Node node = getNodeById(ID);
                foreach (Attribute attr in node.attributes)
                {
                    if (attr.name == "name")
                    {
                        drawString(node.ID.ToString(), Brushes.Yellow, mainDepth - 1, (columnWidth) * (deepnessRate - 1) + (columnWidth), currentH * h);
                        drawString(attr.value, Brushes.Cyan, mainDepth, (columnWidth) * deepnessRate + mainDepth * 2, currentH * h + (mainDepth / 10));
                    }
                    else
                    {
                        drawString(attr.name + ":" + attr.value, mainDepth, (columnWidth) * deepnessRate, currentH * h + (mainDepth / 10));
                    }
                    currentH++;
                }
                //вертикальные линии листочков
                drawLine(Color.WhiteSmoke, 1, (columnWidth) * deepnessRate, currentH * h, (columnWidth) * deepnessRate, (currentH - node.attributes.Count) * h);
                // drawLine(Color.WhiteSmoke, 1, (columnWidth) * (deepnessRate + 1), currentH * h, (columnWidth) * (deepnessRate + 1), (currentH - node.attributes.Count) * h);


                //горизонтальные линии листочков
                drawLine(Color.WhiteSmoke, 1, (columnWidth) * deepnessRate, currentH * h, (columnWidth) * (deepnessRate + 1), currentH * h);

                //вертикальные линии ветвей
                for (int i = 1; i < deepnessRate + 1; i++)
                    drawLine(Color.WhiteSmoke, 1, (columnWidth) * (deepnessRate - i), currentH * h, (columnWidth) * (deepnessRate - i), (currentH - node.attributes.Count) * h);

            }
            else
            {//горизонтальные линии ветвей
                drawLine(Color.WhiteSmoke, 1, (columnWidth) * deepnessRate, currentH * h, (columnWidth) * (deepnessRate + 1), currentH * h);

                Node node = getNodeById(ID);
                if (node.attributes != null)
                {
                    foreach (Attribute attr in node.attributes)
                    {
                        if (attr.name == "name")
                        {
                            //id узла
                            drawString(node.ID.ToString(), Brushes.Yellow, mainDepth - 1, (columnWidth) * (deepnessRate - 1) + (columnWidth), currentH * h + (mainDepth / 10));
                            //имя узла
                            drawString(attr.value, Brushes.Cyan, mainDepth + 1, (columnWidth) * deepnessRate + ((columnWidth) / 4), currentH * h + (mainDepth / 10));
                        }
                        else
                            drawString(attr.name + ":" + attr.value, mainDepth, (columnWidth) * deepnessRate, currentH * h + (mainDepth / 10));
                        currentH++;
                    }
                }
                currentH = currentH - node.attributes.Count;
                deepnessRate++;

                for (int i = 0; i < childrens.Count; i++)
                {
                    if (currentH < 1000)
                    {
                        recurciveAttributeDRAW(childrens[i].ID);
                    }
                    else
                    {
                        log("достигнут предел по высоте таблицы", Color.LightGray);
                        break;
                    }

                }
                deepnessRate--;
                //горизонтальные линии ветвей
                drawLine(Color.WhiteSmoke, 1, (columnWidth) * deepnessRate, currentH * h, (columnWidth) * (deepnessRate + 1), currentH * h);


            }
        }

        private int incGetNodeByID = 0;
        private int recurciveAttributeCountSearch(int ID)
        {
            var childrens = getNodesByparentID(ID);
            if (childrens.Count == 0)
            {
                incGetNodeByID++;
                return getNodeById(ID).attributes.Count;

            }
            else
            {
                int sum = 0;

                for (int i = 0; i < childrens.Count; i++)
                {
                    if (incGetNodeByID < 1000)
                    {
                        sum = sum + recurciveAttributeCountSearch(childrens[i].ID);
                    }
                    else
                    {
                        log("достигнут предел по количеству строк таблицы", Color.LightGray);
                        break;
                    }
                }
                return sum;
            }


        }
        public void refresh()
        {
            picBox.Image = bitmap;
            form1.voidDelegate = new Form1.VoidDelegate(form1.Refresh);
            form1.logBox.Invoke(form1.voidDelegate);
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
        }
        public void drawString(string s, double depth, double x, double y)
        {
            y += Y;
            if (x > picBox.Width)
             picBox.Width = Convert.ToInt16(x); 
            else
            if (y > picBox.Width)
                picBox.Height = Convert.ToInt16(y);
            else
                try
                {
                    g.DrawString(s, new Font("Consolas", Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                }
                catch { }
        }
        /////////////////////////////////Brushes.[Color]
        public void drawString(string s, Brush brush, double depth, double x, double y)
        {
            y += Y;
            if (x > picBox.Width)
                picBox.Width = Convert.ToInt16(x);
            else
            if (y > picBox.Height)
                picBox.Height = Convert.ToInt16(y);
            else
            try
            {
                g.DrawString(s, new Font("Consolas", Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
            }
            catch { }
        }

        public void drawLine(Color col, double depth, double x1, double y1, double x2, double y2)
        {
            y1 += Y;
            y2 += Y;
            if (x1 > picBox.Width)
                picBox.Width = Convert.ToInt16(x1);
            else
            if (x2 > picBox.Width)
                picBox.Width = Convert.ToInt16(x2);
            else
            if (y1 > picBox.Height)
                picBox.Height = Convert.ToInt16(y1);
            else
            if (y2 > picBox.Height)
                picBox.Height = Convert.ToInt16(y2);
            else
                g.DrawLine(new Pen(col, Convert.ToInt16(depth)), Convert.ToInt16(Math.Round(x1)), Convert.ToInt16(Math.Round(y1)), Convert.ToInt16(Math.Round(x2)), Convert.ToInt16(Math.Round(y2)));
        }
        private void log(String s, System.Drawing.Color col)
        {
   //         form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
//            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, col);
        }
    }
}
//⬤
[Serializable]
public class Node
{
    public Node(int ID, int parentID, string almostJson)
    {
        this.ID = ID;
        this.parentID = parentID;

        attributes = new List<Attribute>();
        string[] rawAttributes = almostJson.Split(',');
        for (int i = 0; i < rawAttributes.Length; i++)
        {

            addAttribute(rawAttributes[i]);
        }
    }
    public void addAttribute(string nameANDvalue)
    {
        string[] substrings = nameANDvalue.Split(':');
        string name = substrings[0];
        string value = substrings[1];
        if (substrings.Length > 2)
        {
            for (int j = 2; j < substrings.Length; j++)
            {
                value += ':' + substrings[j];
            }
        }
        attributes.Add(new Attribute(name, value));
    }
    public void addAttribute(string name, string value)
    {
        attributes.Add(new Attribute(name, value));
    }
    public void setValue(string value)
    {
        setAttribute("value", value);
    }
    public void setValue(int value)
    {
        setAttribute("value", value.ToString());
    }
    public bool isChange()
    {
        return Convert.ToBoolean(getAttributeValue("is_change"));
    }
    public string getValue()
    {
        return getAttributeValue("value");
    }
    public string setAttribute(string name, string value)
    {
        var isSuccess = false;
        foreach (Attribute attr in attributes)
        {
            if (attr.name == name)
            {
                attr.value = value;
                isSuccess = true;
            }
        }
        if (isSuccess)
            return "0";
        else
            return "атрибут не найден";
    }
    public string getAttributeValue(string name)
    {
        foreach (Attribute attr in attributes)
        {
            if (attr.name == name)
                return attr.value;
        }
        return null;
    }
    public List<Attribute> attributes;
    public int ID;
    public int parentID;
    public bool isLeaf = false;
}
public class Attribute
{
    public Attribute(string name, string value)
    {
        this.name = name;
        this.value = value;
    }
    public string name;
    public string value;
}

/*public string name;
public int value;
public string category;
public bool is_categorical;
public int min;
public int max;
public string[] categories;
public bool is_change;
public bool is_change_up_or_down;
public bool is_const;*/

