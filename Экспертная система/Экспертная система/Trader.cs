using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Экспертная_система
{
    public class Trader
    {
        MainForm mf;
        public double purchase_limit_amount;
        public double purchase_limit_amount_left = 100;
        public int purchase_limit_interval;
        public bool purchase_limit_timer_enabled = false;
        public DateTime purchase_limit_timer_start;

        public List<Position> positions = new List<Position>();
        public List<Order> buyOrders = new List<Order>();
        public List<Order> sellOrders = new List<Order>();

        public double deposit1 = 0;
        public double deposit2 = 100;

        public double lot;
        public int take_profit;

        public double dues = 0;
        public Trader(MainForm mf, Hyperparameters h)
        {
            this.mf = mf;
            purchase_limit_timer_start = DateTime.Now;
            lot = double.Parse(h.getValueByName("lot"));
            take_profit = int.Parse(h.getValueByName("take_profit"));
            purchase_limit_amount = int.Parse(h.getValueByName("purchase_limit_amount"));
            purchase_limit_interval = int.Parse(h.getValueByName("purchase_limit_interval"));
            purchase_limit_amount_left = purchase_limit_amount;
        }

        public void checkOrders(double bid_top, double ask_top)
        {
            for (int i = 0; i < buyOrders.Count; i++)
            {
                // ордер на покупку оказался дороже рыночной цены - сделка
                if (buyOrders[i].price > ask_top)
                {
                    if (mf.ENV != mf.OPT)
                        mf.log("Buy deal (" + buyOrders[i].price.ToString() + ") order_id:" + buyOrders[i].id.ToString(), Color.Cyan);
                    buy(buyOrders[i].price);
                    var new_position = new Position(lot, buyOrders[i].price, positions.Count + 1);
                    buyOrders.RemoveAt(i);

                    i--;

                    // КАК ТОЛЬКО ОРДЕР НА ПОКУПКУ ИСПОЛНЯЕТСЯ - СОЗДАЁТСЯ ОРДЕР НА ПРОДАЖУ
                    new_position.sell_order_id = createSellOrder(ask_top + take_profit, lot);
                    positions.Add(new_position);
                }
                else
                {
                    // удаление залежавшегося ордера на покупку 
                    if ((DateTime.Now - buyOrders[i].created_at).TotalSeconds > 60)
                    {
                        mf.log("Delete old buy order (" + buyOrders[i].price.ToString() + ") order_id:" + buyOrders[i].id.ToString(), Color.MediumVioletRed);
                        buyOrders.RemoveAt(i);
                    }
                }
            }

            for (int i = 0; i < sellOrders.Count; i++)
            {
                // ордер на продажу оказался дешевле рыночной цены - сделка
                if (sellOrders[i].price < bid_top)
                {
                    string sell_order_id = sellOrders[i].id;
                    if (mf.ENV != mf.OPT) 
                        mf.log("Sell deal ( " + sellOrders[i].price.ToString() + " ) order_id:" + sell_order_id.ToString(), Color.Cyan);

                    sell(sellOrders[i].price);

                    // ПОЗИЦИЯ ЗАКРЫТА
                    positions.Find((p) => (p.sell_order_id == sell_order_id)).closed = true;
                    positions.Find((p) => (p.sell_order_id == sell_order_id)).sell_price = sellOrders[i].price;
                    sellOrders.RemoveAt(i);
                    i--;
                }
            }
        }
        /// <summary>
        /// pice = ask_top
        /// </summary>
        /// <param name="ask"></param>
        /// <param name="quantity"></param>
        public void createBuyOrder(double price)
        {
            createBuyOrder(price, DateTime.Now);
        }
        public void createBuyOrder(double price, DateTime time)
        {
            // запрет на создание нескольких ордеров на покупку
            // пока предыдущий ордер не исполнен - следующий создать нельзя
            if (buyOrders.Count > 0)
            {
                return;
            }
            if (!mf.stop_buying && !mf.maintenance_in_progress && !mf.connection_lost)
            {
                if (purchase_limit_amount_left - (price * lot) > 0)
                {
                    buyOrders.Add(new Order(OrderTypes.BUY, lot, price, (mf.ENV == mf.REAL)));
                    if (mf.ENV != mf.OPT)
                        mf.log("BUY ORDER (" + price.ToString() + "): " + buyOrders.Last().id.ToString(), Color.Cyan);

                    purchase_limit_amount_left -= price * lot;
                }
                else
                {
                    if (purchase_limit_timer_enabled == false)
                    {
                        purchase_limit_timer_start = time;
                        purchase_limit_timer_enabled = true;
                        if (mf.ENV != mf.OPT) 
                            mf.log("Таймер сброса лимита запущен: " + time.ToString());
                    }

                    if (purchase_limit_timer_start.AddMinutes(purchase_limit_interval) < time)
                    {
                        if (mf.ENV != mf.OPT) 
                            mf.log("Сброса лимита: " + time.ToString());

                        purchase_limit_amount_left = purchase_limit_amount;
                        purchase_limit_timer_enabled = false;
                    }
                    else
                    {
                        // mf.log("До сброса лимита: " + (purchase_limit_timer_start.AddMinutes(purchase_limit_interval) - time).ToString());
                    }
                }
            }
        }
        /// <summary>
        /// price = ask_top + take_profit
        /// </summary>
        /// <param name="price"></param>
        /// <param name="quantity"></param>
        public string createSellOrder(double price, double quantity)
        {
            if (!mf.maintenance_in_progress && !mf.connection_lost)
            {
                sellOrders.Add(new Order(OrderTypes.SELL, quantity, price, (mf.ENV == mf.REAL)));
                if (mf.ENV != mf.OPT)
                    mf.log("SELL ORDER (" + price.ToString() + "): " + sellOrders.Last().id.ToString(), Color.Pink);
                return sellOrders.Last().id;
            }
            return "error";
        }
        /// <summary>
        /// Транзакция на виртуальном счёте
        /// </summary>
        /// <param name="ask_top"></param>
        public void buy(double ask_top)
        {

            if (deposit2 >= (ask_top * lot))
            {
                deposit1 = deposit1 + lot - (lot * dues);
                deposit2 = deposit2 - (ask_top * lot);

                mf.vis.markLast("‾BUY", "ask_top");
                if (mf.ENV != mf.OPT)
                {
                    mf.log(DateTime.Now.ToString() + " BUY", Color.Blue);
                    mf.log(ask_top.ToString());
                    mf.log("   USD:" + deposit2.ToString());
                    mf.log("   BTC:" + deposit1.ToString());
                }
            }

            mf.refresh_output();
        }
        /// <summary>
        /// Транзакция на виртуальном счёте
        /// </summary>
        /// <param name="bid_top"></param>
        public void sell(double bid_top)
        {
            if (!mf.maintenance_in_progress && !mf.connection_lost)
                if (deposit1 >= lot)
                {
                    deposit1 = deposit1 - lot;
                    deposit2 = deposit2 + (bid_top * lot) - ((bid_top * lot) * dues);

                    mf.vis.markLast("‾SELL", "bid_top");
                    if (mf.ENV != mf.OPT)
                    {
                        mf.log(DateTime.Now.ToString() + " SELL", Color.Red);
                        mf.log(bid_top.ToString());
                        mf.log("   USD:" + deposit2.ToString());
                        mf.log("   BTC:" + deposit1.ToString());
                    }
                }
            mf.refresh_output();
        }
    }
    public class Order
    {
        ////////////////////////////////
        //  price * quantity = amount //
        ////////////////////////////////

        public OrderTypes type;
        public double quantity;
        public double price;
        public string id;
        public DateTime created_at;

        public bool is_real;

        public Order(OrderTypes type, double quantity, double price, bool is_real)
        {
            this.quantity = quantity;
            this.price = price;

            if (is_real)
            {
                // ПУБЛИКАЦИЯ ОРДЕРА НА БИРЖЕ
                // this.id = id из ответа биржи;
            }
            else
            {
                var n = DateTime.Now;
                id = n.Month.ToString() + '.'+n.Day.ToString() +' '+ n.Hour.ToString() +':'+ n.Minute.ToString() +':'+ n.Second.ToString() + '.' + n.Millisecond.ToString();
                created_at = DateTime.Now;
            }

        }
    }
    public class Position
    {
        public DateTime buyed_at;
        public double quantity;
        public double buy_price;
        public int id;
        public string sell_order_id;
        //public PositionState state;
        public bool closed = false;
        public double sell_price;

        public Position(double quantity, double buy_price, int id)
        {
            this.quantity = quantity;
            this.buy_price = buy_price;
            this.id = id;
            this.buyed_at = DateTime.Now;
        }
    }
    public enum PositionState
    {
        Buying,
        Selling
    }
    public enum OrderTypes
    {
        BUY,
        SELL
    }
}
