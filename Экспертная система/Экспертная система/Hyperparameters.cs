using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Экспертная_система
{
    [Serializable]
    public class Hyperparameters
    {
        public int newNodeIdWillBe = 0;
        public Hyperparameters(MainForm form1, string baseNodeName)
        {
            this.form1 = form1;
            addByParentId(-1, "name:" + baseNodeName);
        }
        /*  public Hyperparameters(Form1 form1)
          {
              this.form1 = form1;
              addByParentId(-1, "name:baseNode");
          }     */

        public Hyperparameters(string path, MainForm form1, bool asFile)
        {
            this.form1 = form1;
            fromJSON(System.IO.File.ReadAllText(path, System.Text.Encoding.Default), -1);
        }
        public Hyperparameters(string JSON, MainForm form1)
        {
            this.form1 = form1;
            fromJSON(JSON, -1);
        }
        [NonSerializedAttribute]
        public MainForm form1;
        public PictureBox picBox;
        public Graphics g;
        public Bitmap bitmap;

        public List<Node> nodes = new List<Node>();
        public int Y = 0;
        public bool lightsOn = false;
        public bool inProcess = false;
        public Hyperparameters Clone()
        {
            return new Hyperparameters(this.toJSON(0), form1);
        }
        public Node getNode(string request)
        {
            string[] nodesStr = request.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            int curID = 0;
            for (int i = 0; i < nodesStr.Length; i++)
            {
                var childrens = getNodesByparentID(curID);
                foreach (Node node in childrens)
                {
                    if (node.getAttributeValue("name") == nodesStr[i])
                    {
                        curID = node.ID;
                    }
                }
            }
            return nodes[curID];
        }
        public void Save()
        {
            System.IO.File.WriteAllText(getValueByName("json_file_path"), toJSON(0), System.Text.Encoding.Default);
        }
        public void Save(string filePath)
        {
            System.IO.File.WriteAllText(filePath, toJSON(0), System.Text.Encoding.Default);
        }
        public string getAttrValue(string request)
        {
            string[] reqStr = request.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string nodeRequest = request.Substring(0, request.IndexOf(reqStr[reqStr.Length - 1]));

            Node node = getNode(nodeRequest);

            return node.getAttributeValue(reqStr[reqStr.Length - 1]);
        }
        public void replaceStringInAllValues(string oldString, string newString)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].getAttributeValue("value") != null)
                    nodes[i].setAttribute("value", nodes[i].getAttributeValue("value").Replace(oldString, newString));
            }
        }
        public void addBranch(Hyperparameters branch, string branchName, int parentId)
        {
            int lastNodeId = newNodeIdWillBe;
            for (int i = 0; i < branch.nodes.Count; i++)
            {
                Node newNode = branch.nodes[i].Clone();
                if (newNode.parentID == -1)
                {
                    newNode.parentID = parentId;
                    newNode.setAttribute("name", branchName);
                }
                else
                    newNode.parentID = lastNodeId + newNode.parentID;

                newNode.ID = lastNodeId + newNode.ID;
                nodes.Add(newNode);

                newNodeIdWillBe++;
            }
        }

        public void deleteBranch(int ID)
        {
            int nodesCount1 = nodes.Count;
            recurciveDelete(ID);

            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].name() == "delete")
                {
                    nodes.RemoveAt(i);
                    newNodeIdWillBe--;
                    i--;
                }
            }
            int nodesCount2 = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ID > ID)
                    nodes[i].ID -= (nodesCount1 - nodesCount2);
                if (nodes[i].parentID > ID)
                    nodes[i].parentID -= (nodesCount1 - nodesCount2);
            }
        }
        void recurciveDelete(int ID)
        {
            var forks = getNodesByparentID(ID);
            foreach (Node fork in forks)
            {
                fork.setAttribute("name", "delete");
                recurciveDelete(fork.ID);
            }
            getNodeById(ID).setAttribute("name", "delete");
        }
        public void addNode(Node node, int parentID)
        {
            Node newNode = node.Clone();
            newNode.parentID = parentID;
            newNode.ID = newNodeIdWillBe;
            nodes.Add(newNode);

            newNodeIdWillBe++;
        }
        ////////////////// ADD //////////////////////////////
        public void addVariable(string name, double min, double max, double value)
        {
            Node newNode = new Node(newNodeIdWillBe, 0);
            newNode.addAttribute("name", name);
            newNode.addAttribute("value", value.ToString().Replace(',', '.'));
            newNode.addAttribute("min", min.ToString().Replace(',', '.'));
            newNode.addAttribute("max", max.ToString().Replace(',', '.'));
            // newNode.addAttribute("step", step.ToString().Replace(',', '.'));
            newNode.addAttribute("variable", "numerical");

            nodes.Add(newNode);

            newNodeIdWillBe++;
        }
        public void addVariable(int parentID, string name, double min, double max, double value)
        {
            Node newNode = new Node(newNodeIdWillBe, parentID);
            newNode.addAttribute("name", name);
            newNode.addAttribute("value", value.ToString().Replace(',', '.'));
            newNode.addAttribute("min", min.ToString().Replace(',', '.'));
            newNode.addAttribute("max", max.ToString().Replace(',', '.'));
            // newNode.addAttribute("step", step.ToString().Replace(',', '.'));
            newNode.addAttribute("variable", "numerical");

            nodes.Add(newNode);

            newNodeIdWillBe++;
        }
        public void addVariable(int parentID, string name, string category, string categories)
        {
            Node newNode = new Node(newNodeIdWillBe, parentID);
            newNode.addAttribute("name", name);
            newNode.addAttribute("value", category);
            newNode.addAttribute("categories", categories);
            newNode.addAttribute("variable", "categorical");

            nodes.Add(newNode);

            newNodeIdWillBe++;
        }

        public int add(string attributes)
        {
            if (!attributes.Contains("name"))
            {  //если в записи атрибутов нет атрибута-имени (например "pathPrefix:D:\\Anton\\Desktop\\MAIN")
                //добавляется атрибут-имя соответсвующее имени единственного атрибута, а его имя заменяется на "value"
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
        //INT
        public void add(string name, int value)
        {
            add("name:" + name + ",value:" + value.ToString());
        }
        public void add(string name, string value)
        {
            Node newNode = new Node(newNodeIdWillBe, 0);
            newNode.addAttribute("name", name);
            newNode.addAttribute("value", value);
            nodes.Add(newNode);
            newNodeIdWillBe++;
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
                for (int i = 0; i < nodes.Count; i++)
                    if (nodes[i].ID == ID)
                        return nodes[i];
                for (int i = ID; i >= 0; i--)
                    if (nodes[i].ID == ID)
                        return nodes[i];
            }
            log("не найден параметр по ID = " + ID.ToString(), System.Drawing.Color.Red);
            return null;
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
                        resnodes.Add(nodes[i].Clone());
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
                    resnodes.Add(nodes[i].Clone());
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
                log("Аттрибут " + name + " не найден", System.Drawing.Color.Yellow);
                return null;
            }
            return node.getAttributeValue("value");
        }
        public List<Node> getNodeByName(string name)
        {
            List<Node> retNodes = new List<Node>();
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i].getAttributeValue("name") == name)
                    retNodes.Add(nodes[i]);
            if (retNodes == null)
            {
                log("Узел " + name + " не найден", System.Drawing.Color.Red);
                return null;
            }
            return retNodes;
        }

        ////////////////// SET //////////////////////////////
        public void setValueByName(string name, int value)
        {
            setValueByName(name, value.ToString());
        }
        public void setValueByName(string name, string value)
        {
            bool setted = false;
            foreach (Node node in getNodeByName(name))
            {
                node.setAttribute("value", value);
                setted = true;
            }
            if (setted == false)
                add(name, value);
        }


        public void draw(int rootId, PictureBox target_pictureBox, int fontDepth, int columnWidth)
        {
            bool isFirstTime = true;
        drawHyperparametersAgain:

            totalAttributesNumber = 0;
            deepnessRate = 0;
            this.h = fontDepth;
            currentH = 0;
            mainDepth = Convert.ToInt16(h * 0.4);
            this.columnWidth = columnWidth;
            // this.form1 = form1;
            incGetNodeByID = 0;
            totalAttributesNumber = recurciveAttributeCountSearch(rootId);
            picBox = target_pictureBox;
            // picBox.Height = h * totalAttributesNumber+400;

            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
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

            for (int i = 1; i < jsonText.Length - 1; i++)
                if (jsonText[i] == '\\' && jsonText[i - 1] != '\\' && jsonText[i + 1] != '\\')
                {
                    var a = jsonText.Substring(0, i) + '\\';
                    var b = jsonText.Substring(i, jsonText.Length - i);
                    jsonText = a + b;
                }
            return '{' + jsonText + '}';
        }
        private List<int> namesParentID = new List<int>();
        private List<string> names = new List<string>();
        private List<string> namesWhithIndex = new List<string>();
        string buildJSON(int ID)
        {
            string s = "";

            var childrens = getNodesByparentID(ID);
            if (childrens.Count == 0)
            {
                Node node = getNodeById(ID);

                string nodeName = node.getAttributeValue("name");

                if (names.Contains(nodeName))
                {
                    if (namesParentID[names.IndexOf(nodeName)] == node.parentID)
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
                        namesParentID[names.IndexOf(nodeName)] = node.parentID;
                }
                else
                {
                    names.Add(nodeName);
                    namesParentID.Add(node.parentID);
                }
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
                namesParentID.Clear();
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
                if (RAWattr[i].Split(':').Length == 1)
                {
                    newNode.attributes[newNode.attributes.Count - 1].value += ',' + RAWattr[i];
                }
                else
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
                bool isVariable = false;
                if (node.getAttributeValue("variable") != null)
                {
                    isVariable = true;
                }
                foreach (Attribute attr in node.attributes)
                {
                    if (attr.name == "name")
                    {
                        drawString(node.ID.ToString(), Brushes.Yellow, mainDepth - 1, (columnWidth) * (deepnessRate - 1) + (columnWidth), currentH * h);
                        drawString(attr.value, Brushes.Cyan, mainDepth, (columnWidth) * deepnessRate + mainDepth * 2, currentH * h + (mainDepth / 10));
                    }
                    else if (isVariable && attr.name == "value")
                    {
                        drawString(attr.name + ":" + attr.value, Brushes.Lime, mainDepth, (columnWidth) * deepnessRate, currentH * h + (mainDepth / 10));
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
            form1.voidDelegate = new MainForm.VoidDelegate(form1.Refresh);
            form1.logBox.Invoke(form1.voidDelegate);

        }

        public delegate void DrawStringDelegate(string s, double depth, double x, double y);
        public void drawString(string s, double depth, double x, double y)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawStringDelegate(drawString), new Object[] { s, depth, x, y }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
            {
                // код обновления состояния контрола

                y += Y;
                if (x > picBox.Width)
                    picBox.Width = Convert.ToInt16(x);
                else
                if (y > picBox.Height)
                    picBox.Height = Convert.ToInt16(y);
                else
                    try
                    {
                        if (lightsOn)
                        {
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                        }
                        else
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                    }
                    catch { }
            }
        }

        public delegate void DrawStringDelegate2(string s, Brush brush, double depth, double x, double y);
        /////////////////////////////////Brushes.[Color]
        public void drawString(string s, Brush brush, double depth, double x, double y)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawStringDelegate2(drawString), new Object[] { s, brush, depth, x, y }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
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
                        if (lightsOn)
                        {
                            if (brush == Brushes.White)
                                g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.Black, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                            else
                                g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                        }
                        else
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), brush, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))));
                    }
                    catch { }
            }
        }

        public delegate void DrawLineDelegate(Color col, double depth, double x1, double y1, double x2, double y2);
        public void drawLine(Color col, double depth, double x1, double y1, double x2, double y2)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawLineDelegate(drawLine), new Object[] { col, depth, x1, y1, x2, y2 }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
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
        }
        private void log(String s, Color col)
        {
            form1.log(s, col);
        }
        public void log(string s)
        {
            form1.log(s);
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
            if (rawAttributes[i].Split(':').Length == 1)
            {
                attributes[attributes.Count - 1].value += ',' + rawAttributes[i];
            }
            else
            {
                addAttribute(rawAttributes[i]);
            }
        }
    }
    public Node(int ID, int parentID)
    {
        this.ID = ID;
        this.parentID = parentID;

        attributes = new List<Attribute>();
    }
    public Node Clone()
    {
        string attributesString = "";
        foreach (Attribute attr in attributes)
            attributesString += attr.name + ':' + attr.value + ',';
        attributesString = attributesString.Remove(attributesString.Length - 1, 1);
        Node newNode = new Node(this.ID, this.parentID, attributesString);

        return newNode;
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
    public string name()
    {
        return getAttributeValue("name");
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

