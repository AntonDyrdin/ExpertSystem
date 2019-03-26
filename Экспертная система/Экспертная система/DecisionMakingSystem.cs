using System.Collections.Generic;
using System.Linq;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Экспертная_система
{
    public class DecisionMakingSystem
    {


        public List<State> S;
        public List<DMSParameter> parameters;
        public Form1 form1;
        public State ActualState;
        public DMSAction lastAction;
        public List<DMSAction> defaultActions;
        public State lastState;
        Random r;
        public double epsilon = 0.05;
        public double alpha = 0.9;
        public double gamma = 0.1;

        public PictureBox picBox;
        public Graphics g;
        public Bitmap bitmap;
        public bool lightsOn = false;

        public int mainDepth = 14;
        public DecisionMakingSystem(Form1 form1)
        {
            r = new Random();
            this.form1 = form1;
            S = new List<State>();
            parameters = new List<DMSParameter>();
            defaultActions = new List<DMSAction>();

        }
        public void drawQ()
        {
            this.picBox = form1.picBox;
            bitmap = new Bitmap(picBox.Width, picBox.Height);
            g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;

            int betweenLines = mainDepth;
            int emptyXGap = 100;

            int symbolsInValue = 5;

            for (int i = 0; i < S[0].p.Count(); i++)
            {
                drawLine(Color.Gray, 1, emptyXGap + (mainDepth * symbolsInValue) * i, 0, emptyXGap + (mainDepth * symbolsInValue) * i, (S.Count + 1) * (mainDepth + betweenLines));

                drawString(S[0].p[i].name, mainDepth, emptyXGap + (mainDepth * symbolsInValue) * i, betweenLines / 2);
            }

            int xGap = symbolsInValue * mainDepth * S[0].p.Length;

            for (int i = 0; i < S[0].A.Count(); i++)
            {
                drawLine(Color.Gray, 1, emptyXGap + xGap + (mainDepth * symbolsInValue) * i, 0, emptyXGap + xGap + (mainDepth * symbolsInValue) * i, (S.Count + 1) * (mainDepth + betweenLines));

                drawString(S[0].A[i].type, mainDepth, emptyXGap + xGap + (mainDepth * symbolsInValue) * i, betweenLines / 2);
            }

            int yGap = 10 + mainDepth + 5;

            for (int i = 0; i < S.Count; i++)
            {
                drawLine(Color.Gray, 1, emptyXGap, yGap + i * (mainDepth + betweenLines), mainDepth * S[0].p.Count() * S[i].A.Count * symbolsInValue, yGap + i * (mainDepth + betweenLines));

                for (int k = 0; k < S[0].p.Count(); k++)
                {
                    if (S[i].p[k].value.Length > symbolsInValue)
                        drawString(S[i].p[k].value.Substring(0, symbolsInValue - 1), mainDepth, emptyXGap + (mainDepth * symbolsInValue) * k, yGap + i * (mainDepth + betweenLines) + betweenLines / 2);
                    else
                        drawString(S[i].p[k].value, mainDepth, emptyXGap + (mainDepth * symbolsInValue) * k, yGap + i * (mainDepth + betweenLines) + betweenLines / 2);
                }

                for (int j = 0; j < S[i].A.Count; j++)
                {
                    if (S[i].A[j].Q.ToString().Length > symbolsInValue)
                        drawString(S[i].A[j].Q.ToString().Substring(0, symbolsInValue - 1), mainDepth, emptyXGap + xGap + j * (mainDepth * symbolsInValue), yGap + i * (mainDepth + betweenLines) + betweenLines / 2);
                    else
                        drawString(S[i].A[j].Q.ToString(), mainDepth, emptyXGap + xGap + j * (mainDepth * symbolsInValue), yGap + i * (mainDepth + betweenLines) + betweenLines / 2);
                }
            }

            picBox.Image = bitmap;
        }
        public void setR(double r)
        {
            //lastAction.Q = Q;
            //  lastAction.Q = (lastAction.Q * (lastAction.attemptsNumber - 1) + r) / lastAction.attemptsNumber;
            double Qmax = 0;

            DMSAction action;
            for (int k = 0; k < ActualState.A.Count; k++)
            {
                if (Qmax < ActualState.A[k].Q)
                {
                    action = ActualState.A[k];
                    Qmax = ActualState.A[k].Q;
                }
            }

            lastAction.Q = lastAction.Q + alpha * (r + gamma * Qmax - lastAction.Q);

          //  drawQ();


            //  log(lastAction.Q.ToString());
        }

        public void setActualState(string str)
        {
            lastState = ActualState;
            ActualState = getStateByString(str);
        }

        public void setActualState(State state)
        {
            lastState = ActualState;
            ActualState = state;
        }
        public DMSAction getAction(string str)
        {
            return getAction(getStateByString(str));
        }
        public DMSAction getAction(State state)
        {

            DMSAction action = null;

            int indexOfActualState = S.IndexOf(ActualState);

            if (lastAction != null)
            {
                //построение цепи Маркова
                //   lastAction.jumpCounter[indexOfActualState]++;
                //    lastAction.ProbabilityOfNextStates[indexOfActualState] = lastAction.jumpCounter[indexOfActualState] / lastAction.attemptsNumber;
                ///////////////////////////                                               
            }

            /*string s = "S" + i.ToString()+ " = ";
            for (int k = 0; k < tempState.p.Length; k++)
            {
                s += tempState.p[k].value + ',';
            }
            log(s); */

            double Qmax = double.MinValue;
            double CountMin = double.MaxValue;
            if (r.NextDouble() < epsilon)
            {
                //исследование
                for (int k = 0; k < S[indexOfActualState].A.Count; k++)
                {
                    if (CountMin > S[indexOfActualState].A[k].attemptsNumber)
                    {
                        action = S[indexOfActualState].A[k];
                        CountMin = S[indexOfActualState].A[k].attemptsNumber;
                    }
                }
            }
            else
            {
                //эксплуатация
                for (int k = 0; k < S[indexOfActualState].A.Count; k++)
                {
                    if (Qmax < S[indexOfActualState].A[k].Q)
                    {
                        action = S[indexOfActualState].A[k];
                        Qmax = S[indexOfActualState].A[k].Q;
                    }
                }
            }

            if (action == null)
            {
                action.type = "";
            }
            action.attemptsNumber++;
            lastAction = action;

            //   log(action.type);
            return action;
        }
        public void addParameter(string name, string values)
        {
            string[] splittedValues = values.Split(',');
            DMSParameter p = new DMSParameter(name);
            p.values = splittedValues;


            parameters.Add(p);
        }

        private State tempState;
        public void generateStates()
        {
            tempState = new State(defaultActions);
            tempState.p = new DMSParameter[parameters.Count];
            for (int k = 0; k < tempState.p.Length; k++)
            {
                tempState.p[k] = new DMSParameter(parameters[k].name);
                tempState.p[k].value = parameters[k].value;
                tempState.p[k].values = new string[parameters[k].values.Length];
                for (int q = 0; q < parameters[k].values.Length; q++)
                    tempState.p[k].values[q] = parameters[k].values[q];
            }
            parametersCombinationSearch(0);

            for (int k = 0; k < S.Count; k++)
            {
                for (int j = 0; j < S[k].A.Count; j++)
                {
                    S[k].A[j].jumpCounter = new int[S.Count];
                    S[k].A[j].ProbabilityOfNextStates = new int[S.Count];
                }

            }
        }
        private void parametersCombinationSearch(int parameterIndex)
        {

            for (int j = 0; j < parameters[parameterIndex].values.Length; j++)
            {
                tempState.p[parameterIndex].value = tempState.p[parameterIndex].values[j];

                if (parameterIndex < parameters.Count - 1)
                {
                    parametersCombinationSearch(parameterIndex + 1);
                }
                else
                {
                    S.Add(tempState);

                   /* string s = "S" + S.Count() + " = ";
                    for (int k = 0; k < tempState.p.Length; k++)
                    {
                        s += tempState.p[k].value + ',';
                    }
                    log(s);*/

                    if (S.Count == 0)
                    {
                        tempState = new State(defaultActions);
                        tempState.p = new DMSParameter[parameters.Count];
                        for (int k = 0; k < tempState.p.Length; k++)
                        {
                            tempState.p[k] = new DMSParameter(parameters[k].name);
                            tempState.p[k].value = parameters[k].value;
                            tempState.p[k].values = new string[parameters[k].values.Length];
                            for (int q = 0; q < parameters[k].values.Length; q++)
                                tempState.p[k].values[q] = parameters[k].values[q];
                        }
                    }
                    else
                    {
                        var tempState1 = tempState;
                        tempState = new State(defaultActions);
                        tempState.p = new DMSParameter[parameters.Count];
                        for (int k = 0; k < tempState.p.Length; k++)
                        {
                            tempState.p[k] = new DMSParameter(tempState1.p[k].name);
                            tempState.p[k].value = tempState1.p[k].value;
                            tempState.p[k].values = new string[tempState1.p[k].values.Length];
                            for (int q = 0; q < tempState1.p[k].values.Length; q++)
                                tempState.p[k].values[q] = tempState1.p[k].values[q];
                        }
                    }
                }
            }
        }
        public State getStateByString(string str)
        {
            string[] splitted = str.Split(',');

            var tempState = new State(defaultActions);

            tempState.p = new DMSParameter[parameters.Count];

            for (int k = 0; k < tempState.p.Length; k++)
            {
                tempState.p[k] = new DMSParameter(parameters[k].name);
                tempState.p[k].value = parameters[k].value;
                tempState.p[k].values = new string[parameters[k].values.Length];
                for (int q = 0; q < parameters[k].values.Length; q++)
                    tempState.p[k].values[q] = parameters[k].values[q];
            }
            foreach (string s in splitted)
            {
                foreach (DMSParameter p in tempState.p)
                {
                    if (p.name == s.Split(':')[0])
                    {
                        p.value = s.Split(':')[1];
                    }
                }
            }
            for (int i = 0; i < S.Count(); i++)
            {
                bool isItThatStateWhatWeSearch = true;
                for (int j = 0; j < S[i].p.Length; j++)
                {
                    if (S[i].p[j].value != tempState.p[j].value)
                    {
                        isItThatStateWhatWeSearch = false;
                    }
                }
                if (isItThatStateWhatWeSearch)
                {
                    return S[i];
                }
            }
            return null;

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
        public void drawStringVertical(string s, double depth, double x, double y)
        {
            if (picBox.InvokeRequired)
            {
                picBox.Invoke(new DrawStringDelegate(drawString), new Object[] { s, depth, x, y }); // вызываем эту же функцию обновления состояния, но уже в UI-потоке
            }
            else
            {
                if (y > picBox.Height)
                    picBox.Height = Convert.ToInt16(y);
                else
                    try
                    {
                        if (lightsOn)
                        {
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.Black, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))), new StringFormat(StringFormatFlags.DirectionVertical));
                        }
                        else
                            g.DrawString(s, new Font(form1.logBox.Font.Name, Convert.ToInt16(depth)), Brushes.White, new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y))), new StringFormat(StringFormatFlags.DirectionVertical));
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

                if (y1 > picBox.Height)
                    picBox.Height = Convert.ToInt16(y1);
                else
                if (y2 > picBox.Height)
                    picBox.Height = Convert.ToInt16(y2);
                else
                    g.DrawLine(new Pen(col, Convert.ToInt16(depth)), Convert.ToInt16(Math.Round(x1)), Convert.ToInt16(Math.Round(y1)), Convert.ToInt16(Math.Round(x2)), Convert.ToInt16(Math.Round(y2)));
            }
        }
        void log(string s)
        {
            form1.logDelegate = new Form1.LogDelegate(form1.delegatelog);
            form1.logBox.Invoke(form1.logDelegate, form1.logBox, s, System.Drawing.Color.White);
        }
    }
    public class DMSAction
    {
        public int[] ProbabilityOfNextStates;
        public int[] jumpCounter;

        public string type;
        public int attemptsNumber;
        public double Q;
        public DMSAction(string type)
        {
            this.type = type;
            attemptsNumber = 0;
        }
        public DMSAction(string type, int statesNumber)
        {
            ProbabilityOfNextStates = new int[statesNumber];
            jumpCounter = new int[statesNumber];
            this.type = type;
            attemptsNumber = 0;
        }
    }
    public class State
    {
        public DMSParameter[] p;
        public List<DMSAction> A;
        public State(List<DMSAction> defaultActions)
        {
            A = new List<DMSAction>();
            for (int i = 0; i < defaultActions.Count; i++)
            {
                A.Add(new DMSAction(defaultActions[i].type));
            }
        }
    }
    public class DMSParameter
    {

        public string name;
        public string[] values;
        public string value;

        public DMSParameter(string name)
        {
            this.name = name;
        }
    }
}