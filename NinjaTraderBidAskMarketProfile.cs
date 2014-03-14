 #region Using declarations
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using System.Collections.Generic;
#endregion

namespace NinjaTrader.Indicator
{
   public class AAAAAAABidAndAskSize : Indicator
   {
                string _Version = "04/27/2012 15:20:00";

                //Smaller number makes for a longer line
                int _Divisor = 5000000;
                int TNA_Setting = 5000000;
                int DIA_Setting = 5000000;
                int SPY_Setting = 1000000;
                int QQQ_Setting = 2500000; //3000000
                int INTC_Setting = 1000000;
                int ZN_Setting = 5000000;
                int ZB_Setting = 5000000;
                DataSeries _BodySize;


                List<PotentialTrade> PotentialTradesList = new List<PotentialTrade>();
                SortedList<int, SortedList<double, QuoteDataAtPrice>>
                BarDataCollection  = new SortedList<int, SortedList<double, QuoteDataAtPrice>>();
                protected override void Initialize()
                {
                        Overlay = true;
                        CalculateOnBarClose = false;
                        ZOrder = -1;

                        if (this.Instrument.FullName == "TNA")
                                _Divisor = TNA_Setting;
                        else if (this.Instrument.FullName == "DIA")
                                _Divisor = DIA_Setting;
                        else if (this.Instrument.FullName == "SPY")
                                _Divisor = SPY_Setting;
                        else if (this.Instrument.FullName == "QQQ")
                                _Divisor = QQQ_Setting;
                        else if (this.Instrument.FullName == "INTC")
                                _Divisor = INTC_Setting;
                        else if (this.Instrument.FullName.Contains("ZB "))
                                _Divisor = ZN_Setting;
                        else if (this.Instrument.FullName.Contains("ZN "))
                                _Divisor = ZB_Setting;

                        _BodySize = new DataSeries(this);
                }

                int _CurrentBar = -1;
                StatisticHolder _LongStatisticHolder = new StatisticHolder("LONG");
                StatisticHolder _ShortStatisticHolder = new StatisticHolder("SHORT");
                protected override void OnBarUpdate()
                {



                        if (_CurrentBar != CurrentBar)
                        {

                                if (BarDataCollection.Count > 0)
                                {
                                int lastBarIndexInCollection = BarDataCollection.Count - 1;
                                SortedList<double, QuoteDataAtPrice> LastSL =
BarDataCollection[lastBarIndexInCollection];
                                if (LastSL.Count > 0)
                                {
                                        double thePriceOfLargestSizeAtBidAndAtAsk =
GetPriceOfLargestSize(lastBarIndexInCollection,
dataType.bidAndAskSize);
                                        double theLargestSizeSinged =
GetLargestSizeSigned(lastBarIndexInCollection,
dataType.bidAndAskSize);

                                        int percentageOfBids = 0;
                                        int percentageOfAsks = 0;
                                        percentageOfBids = GetPercentageOfBids(lastBarIndexInCollection,
dataType.bidAndAskSize);
                                        percentageOfAsks = 100 - percentageOfBids;

                                        int percentageOfBidsAtTop = 0;
                                        int percentageOfAsksAtTop = 0;
                                        percentageOfBidsAtTop =
GetPercentageOfBidsAtTopOrBottom(lastBarIndexInCollection,
dataType.bidAndAskSize, Median[1], TopOrBottomEnum.Top);
                                        percentageOfAsksAtTop = 100 - percentageOfBidsAtTop;

                                        int percentageOfBidsAtBottom = 0;
                                        int percentageOfAsksAtBottom = 0;
                                        percentageOfBidsAtBottom =
GetPercentageOfBidsAtTopOrBottom(lastBarIndexInCollection,
dataType.bidAndAskSize, Median[1], TopOrBottomEnum.Bottom);
                                        percentageOfAsksAtBottom = 100 - percentageOfBidsAtBottom;

                                        int percentageOfDataAtTop = 0;
                                        int percentageOfDataAtBottom = 0;
                                        percentageOfDataAtTop =
GetPercentageOfDataAtTopHalfOfBar(lastBarIndexInCollection,
dataType.bidAndAskSize, Median[1]);
                                        percentageOfDataAtBottom = 100 - percentageOfDataAtTop;

                                        if (thePriceOfLargestSizeAtBidAndAtAsk > 0)
                                        {

                                                if (thePriceOfLargestSizeAtBidAndAtAsk > Median[1]
                                                        && (Open[1] > Median[1])
                                                        && IsTopOrBottomQuarter(Close[1], High[1], Low[1],TopOrBottomEnum.Bottom)
                                                        && Close[1] < EMA(Close,20)[1])
                                                {
                                                        //Potential short

//                                                       if (theLargestSizeSinged > 0)
//                                                      {
//                                                              BackColor = Color.MistyRose;
//                                                              PrintAlert(Time[1], "FULL SHORT");
//                                                      }
//                                                      else
//                                                      {
//                                                              PrintAlert(Time[1], "PARTIAL SHORT");
//                                                              BackColor = Color.LightGray;
//                                                      }

                                                        PotentialTrade newTrade = new PotentialTrade();
                                                        newTrade.SignalBar = lastBarIndexInCollection;
                                                        newTrade.SignalPrice = thePriceOfLargestSizeAtBidAndAtAsk;
                                                        newTrade.theTradeDirection = TradeDirection.Short;
                                                        newTrade.StopPrice = High[1] + TickSize;
                                                        newTrade.theTradeState = TradeState.Closed;
                                                        newTrade.ProfitPrice =  newTrade.SignalPrice -
(Math.Abs(newTrade.SignalPrice - newTrade.StopPrice) * 2);
                                                        PotentialTradesList.Add (newTrade);


//                                                       Print ("SHORT - Close[1] = " + Close[1]
//                                                      + " theLargestSizeAtBidAndAtAsk = " + thePriceOfLargestSizeAtBidAndAtAsk
//                                                      + " High = " +  High[1]
//                                                      + " Low = " +   Low[1]
//                                                      + " Time = " +   Time[1]);
                                                }
                                                else if  (thePriceOfLargestSizeAtBidAndAtAsk  < Median[1]
                                                        && (Open[1] < Median[1])
                                                        && IsTopOrBottomQuarter(Close[1], High[1], Low[1], TopOrBottomEnum.Top)
                                                        &&  Close[1] > EMA(Close,20)[1])
                                                {
                                                 //ALL - Close[1] = 139.77
                                                        //theLargestSizeAtBidAndAtAsk = 139.65
                                                        //High = 139.77
                                                        //Low = 139.59
                                                        //Close = 139.77
                                                        //Median = 139.68
                                                        //EMA20 = 139.656132929072
                                                        //Time = 4/4/2012 2:35:00 PM


//                                                       if (theLargestSizeSinged < 0)
//                                                      {
//                                                              BackColor = Color.LightCyan;
//                                                              PrintAlert(Time[1], "FULL LONG");
//                                                      }
//                                                      else
//                                                      {
//                                                              BackColor = Color.LightGray;
//                                                              PrintAlert(Time[1], "PARTIAL LONG");
//                                                      }

                                                        PotentialTrade newTrade = new PotentialTrade();
                                                        newTrade.SignalBar = lastBarIndexInCollection;
                                                        newTrade.SignalPrice = thePriceOfLargestSizeAtBidAndAtAsk;
                                                        newTrade.theTradeDirection = TradeDirection.Long;
                                                        newTrade.StopPrice = Low[1] - TickSize;
                                                        newTrade.theTradeState = TradeState.Closed;
                                                        newTrade.ProfitPrice = newTrade.SignalPrice +
(Math.Abs(newTrade.SignalPrice - newTrade.StopPrice) * 2);
                                                        PotentialTradesList.Add (newTrade);

                                                }

                                                PrintAlert(Time[1], "percentageOfAsksAtTop = " +
percentageOfAsksAtTop + "  percentageOfBidsAtTop = " +
percentageOfBidsAtTop);
                                                PrintAlert(Time[1], "percentageOfAsksAtBottom = " +
percentageOfDataAtBottom + "  percentageOfBidsAtBottom = " +
percentageOfDataAtTop);
                                                PrintAlert(Time[1], "percentageOfDataAtBottom = " +
percentageOfDataAtBottom + "  percentageOfDataAtTop = " +
percentageOfDataAtTop);
                                                PrintAlert(Time[1], "percentageOfBids = " + percentageOfBids + "
 percentageOfAsks = " +  percentageOfAsks);
                                                PrintAlert(Time[1], "percentageOfDataAtBottom = " +
percentageOfDataAtBottom + "  percentageOfDataAtTop = " +
percentageOfDataAtTop);
                                                PrintAlert(Time[1], "thePriceOfLargestSizeAtBidAndAtAsk = " +
thePriceOfLargestSizeAtBidAndAtAsk);
                                                PrintAlert(Time[1], "IsBarAerodynamic Bullish = " +
IsBarAerodynamic(1, MarketDirectionEnum.Bullish,
DirectionalFilterEnum.VeryStrong));
                                                PrintAlert(Time[1], "IsBarAerodynamic Bearish = " +
IsBarAerodynamic(1, MarketDirectionEnum.Bearish,
DirectionalFilterEnum.VeryStrong));


//04/24/2012 01:55:00 - QQQ (5 min):  percentageOfAsksAtTop = 42
percentageOfBidsAtTop = 58
//04/24/2012 01:55:00 - QQQ (5 min):  percentageOfAsksAtBottom = 13
percentageOfBidsAtBottom = 87
//04/24/2012 01:55:00 - QQQ (5 min):  percentageOfDataAtBottom = 13
percentageOfDataAtTop = 87
//04/24/2012 01:55:00 - QQQ (5 min):  percentageOfBids = 59
percentageOfAsks = 41


                                                // && IsTopOrBottomQuarter(Close[1], High[1], Low[1], TopOrBottomEnum.Top)
                                                ///BULL
                                                if (percentageOfBids > percentageOfAsks
                                                        && (percentageOfDataAtTop) < percentageOfDataAtBottom
                                                        && thePriceOfLargestSizeAtBidAndAtAsk  < (Open[1] + Close[1]) / 2
                                                        && IsBarAerodynamic(1, MarketDirectionEnum.Bullish,
DirectionalFilterEnum.VeryStrong)
                                                        && Close[1] > EMA(Close,20)[1]
                                                        && Math.Abs(Open[1] - Close[1]) > EMA(_BodySize, 3)[1])
                                                {
                                                        //Bull breakout
                                                        DrawDiamond("Long" + CurrentBar, true, 1, Low[1] - (TickSize *
4), Color.LightCyan);
                                                        PrintAlert(Time[1],"LONG");
                                                }
                                                //BEAR
                                                else if (percentageOfBids < percentageOfAsks
                                                        && (percentageOfDataAtBottom) < percentageOfDataAtTop
                                                        && thePriceOfLargestSizeAtBidAndAtAsk >  (Open[1] + Close[1]) / 2
                                                        && IsBarAerodynamic(1, MarketDirectionEnum.Bearish,
DirectionalFilterEnum.VeryStrong)
                                                        && Close[1] < EMA(Close,20)[1]
                                                        && Math.Abs(Open[1] - Close[1]) > EMA(_BodySize, 3)[1])
                                                {
                                                        //Bear breakout
                                                        DrawDiamond("Shprt" + CurrentBar, true, 1, High[1] + (TickSize
* 4), Color.MistyRose);
                                                        PrintAlert(Time[1],"SHORT");
                                                }
                                        }
                                }
                                }
                                _CurrentBar = CurrentBar;
                                BarDataCollection.Add(CurrentBar, new SortedList<double,
QuoteDataAtPrice>());
                                _BodySize.Set (Math.Abs(Open[1] - Close[1]));
                        }
                }

       public override void Plot(Graphics graphics, Rectangle bounds,
double min, double max)
       {
           foreach( KeyValuePair<int, SortedList<double,
QuoteDataAtPrice>> kvp in BarDataCollection)
           {
                                int barIndex = kvp.Key;
                SortedList<double, QuoteDataAtPrice> currentBarDataSL
= kvp.Value;

                                double theLargestSizeAtBidAndAtAsk = GetLargestSize(barIndex,
dataType.bidAndAskSize);



                                foreach (KeyValuePair<double, QuoteDataAtPrice> kvp2 in currentBarDataSL)
                                {
                                        if (kvp2.Value == null)
                                                return;

                                         if (theLargestSizeAtBidAndAtAsk  == 0)
                                                return;

                                        BidAndAskSizeDifference(graphics, kvp2.Value , _Divisor,
theLargestSizeAtBidAndAtAsk);
                                }
                        }
        }


        public void BidAndAskSizeDifference(Graphics graphics,
QuoteDataAtPrice theQuoteDataAtPrice, int Divisor, double
LargestValue)
        {
//
//                    if ((ToTime(Time[0]) < 9350) || (ToTime(Time[0]) > 15500))
//                      return;

                //Bid size ask size difference


                                         float scaledValue = (float) (
Math.Abs((theQuoteDataAtPrice.TotalBidSize -
theQuoteDataAtPrice.TotalAskSize) / Divisor)); //QQQ settings

                                        int tempLineThickness = 3;
//                                      if (( Math.Abs(theQuoteDataAtPrice.TotalBidSize -
theQuoteDataAtPrice.TotalAskSize) == LargestValue)
//                                              && ()
//                                              tempLineThickness = 3;
                double halfPoint = ((theQuoteDataAtPrice.High -
theQuoteDataAtPrice.Low) / 2) + theQuoteDataAtPrice.Low;

                tempLineThickness = 2;

                if (LargestValue == Math.Abs((theQuoteDataAtPrice.TotalBidSize -
theQuoteDataAtPrice.TotalAskSize)))
                        tempLineThickness = 3;

                        graphics.DrawLine(new Pen(Color.Red, tempLineThickness),
ChartControl.GetXByBarIdx(this.Bars,
theQuoteDataAtPrice.CurrentBarNumber ) ,
                                                ChartControl.GetYByValue(this.Bars, theQuoteDataAtPrice.Price),
                                                (ChartControl.GetXByBarIdx(this.Bars,
theQuoteDataAtPrice.CurrentBarNumber )) ,
                                                ChartControl.GetYByValue(this.Bars, theQuoteDataAtPrice.Price));

                                        if (theQuoteDataAtPrice.TotalBidSize > theQuoteDataAtPrice.TotalAskSize)
                                        {
                                                graphics.DrawLine(new Pen(Color.Red, tempLineThickness),
ChartControl.GetXByBarIdx(this.Bars,
theQuoteDataAtPrice.CurrentBarNumber ) ,
                                                ChartControl.GetYByValue(this.Bars, theQuoteDataAtPrice.Price),
                                                (ChartControl.GetXByBarIdx(this.Bars,
theQuoteDataAtPrice.CurrentBarNumber)) +  scaledValue ,
                                                ChartControl.GetYByValue(this.Bars, theQuoteDataAtPrice.Price));
                                        }
                                        else
                                        {
                                                graphics.DrawLine(new Pen(Color.Blue, tempLineThickness),
ChartControl.GetXByBarIdx(this.Bars,theQuoteDataAtPrice.CurrentBarNumber
) ,
                                                        ChartControl.GetYByValue(this.Bars, theQuoteDataAtPrice.Price),
                                                        (ChartControl.GetXByBarIdx(this.Bars,
theQuoteDataAtPrice.CurrentBarNumber )) + (scaledValue),
                                                        ChartControl.GetYByValue(this.Bars, theQuoteDataAtPrice.Price));
                                        }
        }


                        public double _LastTradePrice = 0;
                public double _LastBidPrice = 0;
                public double _LastAskPrice = 0;
                protected override void OnMarketData(MarketDataEventArgs e)
                {
                        if (e.MarketDataType == MarketDataType.Bid)
                        {
                                double quoteDrift = 0;
                                if ((_LastBidPrice != e.Price) && (_LastTradePrice > 0))
                                {
                                        quoteDrift = (_LastAskPrice - _LastTradePrice) + (e.Price -
_LastTradePrice);
                                        AddQuoteData(e.Price,quoteDrift, quote.drift, CurrentBar);
                                }

                                AddQuoteData(e.Price, e.Volume, quote.bid, CurrentBar);
                                _LastBidPrice = e.Price;
                        }

                        if (e.MarketDataType == MarketDataType.Ask)
                        {
                                double quoteDrift = 0;
                                 if ((_LastAskPrice != e.Price) && (_LastTradePrice > 0))
                                {
                                        quoteDrift =  (e.Price - _LastTradePrice) + (_LastBidPrice -
_LastTradePrice);
                                        AddQuoteData(e.Price,quoteDrift, quote.drift, CurrentBar);
                                }

                                AddQuoteData(e.Price, e.Volume, quote.ask, CurrentBar);
                                _LastAskPrice = e.Price;
                        }

                        if  (e.MarketDataType == MarketDataType.Last)
                        {
                                AddQuoteData(e.Price, e.Volume, quote.last, CurrentBar);
                                _LastTradePrice = e.Price;

                                if (e.Price <= _LastBidPrice)
                                        AddQuoteData(e.Price, e.Volume, quote.lastAtBid, CurrentBar);

                                if (e.Price >= _LastAskPrice)
                                        AddQuoteData(e.Price, e.Volume, quote.lastAtAsk, CurrentBar);
                        }
                }

        #region Helper

                public enum quote{bid, ask, last, lastAtBid, lastAtAsk, drift};
                public void AddQuoteData(double thePrice, double theData, quote
theQuote, int theCurrentBar)
                {

                        if (BarDataCollection.Count == 0)
                                return;

                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[CurrentBar];

                        if ((currentBarDataSL != null) && currentBarDataSL.ContainsKey(thePrice))
                                {
                                        QuoteDataAtPrice theQuoteDataAtPrice = currentBarDataSL[thePrice];
                                        if (theQuote == quote.bid)
                                                theQuoteDataAtPrice.TotalBidSize += theData;
                                        else if (theQuote == quote.ask)
                                                theQuoteDataAtPrice.TotalAskSize += theData;
                                        else if (theQuote == quote.last)
                                                theQuoteDataAtPrice.TotalTradeSize += theData;
                                        else if (theQuote == quote.lastAtBid)
                                                theQuoteDataAtPrice.TotalTradeSizeAtBid += theData;
                                        else if (theQuote == quote.lastAtAsk)
                                                theQuoteDataAtPrice.TotalTradeSizeAtAsk += theData;
                                        else if (theQuote == quote.drift)
                                                theQuoteDataAtPrice.Drift += theData;


                                        theQuoteDataAtPrice.CurrentBarNumber = theCurrentBar;
                                        theQuoteDataAtPrice.Price = thePrice;
                                        theQuoteDataAtPrice.High = High[0];
                                        theQuoteDataAtPrice.Low = Low[0];
                                }
                                else
                                {
                                        QuoteDataAtPrice theQuoteDataAtPrice = new QuoteDataAtPrice();
                                        if (theQuote == quote.bid)
                                                theQuoteDataAtPrice.TotalBidSize += theData;
                                        else if (theQuote == quote.ask)
                                                theQuoteDataAtPrice.TotalAskSize += theData;
                                        else if (theQuote == quote.last)
                                                theQuoteDataAtPrice.TotalTradeSize += theData;
                                        else if (theQuote == quote.lastAtBid)
                                                theQuoteDataAtPrice.TotalTradeSizeAtBid += theData;
                                        else if (theQuote == quote.lastAtAsk)
                                                theQuoteDataAtPrice.TotalTradeSizeAtAsk += theData;
                                        else if (theQuote == quote.drift)
                                                theQuoteDataAtPrice.Drift += theData;

                                        theQuoteDataAtPrice.CurrentBarNumber = theCurrentBar;
                                        theQuoteDataAtPrice.Price = thePrice;
                                        theQuoteDataAtPrice.High = High[0];
                                        theQuoteDataAtPrice.Low = Low[0];
                                        currentBarDataSL.Add(thePrice, theQuoteDataAtPrice);
                                }
                }


                public enum dataType{bidSize, askSize, lastTradeSize, lastAtBidSize,
lastAtAskSize, lastAtBidAndAskSize, bidAndAskSize};
                public double GetLargestSize(int theKey, dataType thedataType)
                {
                        double largestValue = 0;
                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[theKey];
                        foreach (KeyValuePair<double, QuoteDataAtPrice> kvp in currentBarDataSL)
                        {
                                if (thedataType == dataType.bidSize)
                                {
                                        if (largestValue < kvp.Value.TotalBidSize)
                                                largestValue = kvp.Value.TotalBidSize;
                                }
                                if (thedataType == dataType.askSize)
                                {
                                        if (largestValue < kvp.Value.TotalAskSize)
                                                largestValue = kvp.Value.TotalAskSize;
                                }
                                if (thedataType == dataType.lastTradeSize)
                                {
                                        if (largestValue < kvp.Value.TotalTradeSize)
                                                largestValue = kvp.Value.TotalTradeSize;
                                }
                                if (thedataType == dataType.lastAtBidSize)
                                {
                                        if (largestValue < kvp.Value.TotalTradeSizeAtBid)
                                                largestValue = kvp.Value.TotalTradeSize;
                                }
                                if (thedataType == dataType.lastAtAskSize)
                                {
                                        if (largestValue < kvp.Value.TotalTradeSizeAtAsk)
                                                largestValue = kvp.Value.TotalTradeSize;
                                }
                                if (thedataType == dataType.lastAtBidAndAskSize)
                                {
                                        if (largestValue < Math.Abs(kvp.Value.TotalTradeSizeAtAsk -
kvp.Value.TotalTradeSizeAtBid))
                                                largestValue = Math.Abs(kvp.Value.TotalTradeSizeAtAsk -
kvp.Value.TotalTradeSizeAtBid);
                                }
                                if (thedataType == dataType.bidAndAskSize)
                                {
                                        if (largestValue < Math.Abs(kvp.Value.TotalAskSize -
kvp.Value.TotalBidSize))
                                                largestValue = Math.Abs(kvp.Value.TotalAskSize - kvp.Value.TotalBidSize);

                                        kvp.Value.MaxValue = largestValue;
                                }


                        }
                        return largestValue;
                }


                public int GetPercentageOfDataAtTopHalfOfBar(int theKey, dataType
thedataType, double MedianPrice)
                {
                        double totalDataAtTop = 0;
                        double totalDataAtBottom = 0;

                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[theKey];
                        foreach (KeyValuePair<double, QuoteDataAtPrice> kvp in currentBarDataSL)
                        {

                                if (thedataType == dataType.bidAndAskSize)
                                {
                                        if (kvp.Value.Price > MedianPrice)
                                                totalDataAtTop += Math.Abs(kvp.Value.TotalAskSize -
kvp.Value.TotalBidSize);
                                        else if (kvp.Value.Price < MedianPrice)
                                                totalDataAtBottom += Math.Abs(kvp.Value.TotalAskSize -
kvp.Value.TotalBidSize);

                                }
                        }

                        if ((totalDataAtTop + totalDataAtBottom) > 0)
                                return (int) ((100 * totalDataAtTop) / (totalDataAtTop +
totalDataAtBottom)) ;

                        return 0;
                }

                public int GetPercentageOfBids(int theKey, dataType thedataType)
                {
                        double totalBids = 0;
                        double totalAsks = 0;

                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[theKey];
                        foreach (KeyValuePair<double, QuoteDataAtPrice> kvp in currentBarDataSL)
                        {

                                if (thedataType == dataType.bidAndAskSize)
                                {
                                        totalBids += kvp.Value.TotalBidSize;
                                        totalAsks += kvp.Value.TotalAskSize;
                                }
                        }

                        if ((totalBids + totalAsks) > 0)
                                return (int) ((100 * totalBids) / (totalBids + totalAsks)) ;

                        return 0;
                }


                public int GetPercentageOfBidsAtTopOrBottom(int theKey, dataType
thedataType, double MedianPrice, TopOrBottomEnum theTopOrBottom)
                {
                        double totalBids = 0;
                        double totalAsks = 0;
                        double percentageOfBids = 0;

                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[theKey];
                        foreach (KeyValuePair<double, QuoteDataAtPrice> kvp in currentBarDataSL)
                        {
                                if (thedataType == dataType.bidAndAskSize)
                                {
                                        if ((theTopOrBottom == TopOrBottomEnum.Top) && (kvp.Value.Price
>= MedianPrice))
                                        {
                                                totalBids += kvp.Value.TotalBidSize;
                                                totalAsks += kvp.Value.TotalAskSize;
                                        }
                                        else if ((theTopOrBottom == TopOrBottomEnum.Bottom) &&
(kvp.Value.Price <= MedianPrice))
                                        {
                                                totalBids += kvp.Value.TotalBidSize;
                                                totalAsks += kvp.Value.TotalAskSize;
                                        }
                                }
                        }

                        if ((totalBids + totalAsks) > 0)
                                return (int) ((100 * totalBids) / (totalBids + totalAsks)) ;

                        return 0;

                }

                public double GetLargestSizeSigned(int theKey, dataType thedataType)
                {
                        double largestValue = 0;
                        double largestValueSigned = 0;
                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[theKey];
                        foreach (KeyValuePair<double, QuoteDataAtPrice> kvp in currentBarDataSL)
                        {

                                if (thedataType == dataType.bidAndAskSize)
                                {
                                        if (largestValue < Math.Abs(kvp.Value.TotalAskSize -
kvp.Value.TotalBidSize))
                                                {
                                                largestValue = Math.Abs(kvp.Value.TotalAskSize - kvp.Value.TotalBidSize);

                                         largestValueSigned = kvp.Value.TotalAskSize - kvp.Value.TotalBidSize;
                                        }
                                }


                        }
                        return largestValueSigned;
                }

                public double GetPriceOfLargestSize(int theKey, dataType thedataType)
                {
                        double priceOfLargestSize = 0;
                        double largestValue = 0;
                        SortedList<double, QuoteDataAtPrice> currentBarDataSL =
BarDataCollection[theKey];
                        foreach (KeyValuePair<double, QuoteDataAtPrice> kvp in currentBarDataSL)
                        {

                                if (thedataType == dataType.bidAndAskSize)
                                {
                                        if (largestValue < Math.Abs(kvp.Value.TotalAskSize -
kvp.Value.TotalBidSize))
                                        {
                                                largestValue = Math.Abs(kvp.Value.TotalAskSize - kvp.Value.TotalBidSize);
                                                priceOfLargestSize = kvp.Value.Price;
                                        }
                                }
                        }
                        return priceOfLargestSize;
                }



        public class QuoteDataAtPrice
                {
                        public int CurrentBarNumber = 0;
                        public double Price = 0;
                        public double High = 0;
                        public double Low = 0;
                        public double TotalBidSize = 0;
                        public double TotalAskSize = 0;
                        public double TotalTradeSize = 0;
                        public double TotalTradeSizeAtBid = 0;
                        public double TotalTradeSizeAtAsk = 0;
                        public double Drift = 0;
                        public double MaxValue = 0;
                }

                public bool IsTopOrBottomThird(double Price, double High, double
Low, TopOrBottomEnum TopOrBottom)
                {
                        if (TopOrBottom == TopOrBottomEnum.Top)
                                return ((((High - Low) * 0.666) + Low) < Price) ? true: false;
                        else
                                return ((((High - Low) * 0.333) + Low) > Price) ? true: false;
                }

                public bool IsTopOrBottomQuarter(double Price, double High, double
Low, TopOrBottomEnum TopOrBottom)
                {
                        if (TopOrBottom == TopOrBottomEnum.Top)
                                return ((((High - Low) * 0.75) + Low) < Price) ? true: false;
                        else
                                return ((((High - Low) * 0.25) + Low) > Price) ? true: false;
                }

                public enum TradeDirection {Long, Short, None};
                public enum TradeState {Pending, Active, Closed};
                public class PotentialTrade
                {
                        public int SignalBar = 0;
                        public TradeDirection theTradeDirection = TradeDirection.None;
                        public double SignalPrice = 0;
                        public TradeState theTradeState = TradeState.Pending;
                        public double StopPrice = 0;
                        public double ProfitPrice = 0;

                }

                public void PrintAlert(DateTime TimeStamp, string AlertMessage)
                {
                        DateTime now = DateTime.Now;
                        Print(TimeStamp.ToString("MM/dd/yyyy hh:mm:ss") + " - " +
this.Instrument.FullName + " (" + Bars.Period.Value.ToString() + "
min):  " + AlertMessage);
                }

                public class StatisticHolder
                {
                        public string Name = "";

                        int TotalBidsVSAsk_BidBigger = 0;
                        int TotalBidsVSAsk_AskBigger = 0;

                        int TotalTopBidsVSAsk_BidBigger = 0;
                        int TotalTopBidsVSAsk_AskBigger = 0;

                        int TotalBottomBidsVSAsk_BidBigger = 0;
                        int TotalBottomBidsVSAsk_AskBigger = 0;

                        int TotalDataAtTop = 0;
                        int TotalDataAtBottom = 0;


                        public StatisticHolder(string Name)
                        {
                                this.Name = Name;
                        }

                        public void AddData(int percentageOfBids, int percentageOfAsks, int
percentageOfBidsAtTop, int percentageOfAsksAtTop,
                                        int percentageOfBidsAtBottom, int percentageOfAsksAtBottom, int
percentageOfDataAtTop , int percentageOfDataAtBottom)
                        {
                                if (percentageOfBids > percentageOfAsks)
                                        this.TotalBidsVSAsk_BidBigger += 1;
                                else if (percentageOfBids < percentageOfAsks)
                                        this.TotalBidsVSAsk_AskBigger += 1;

                                if (percentageOfBidsAtTop > percentageOfAsksAtTop)
                                        this.TotalTopBidsVSAsk_BidBigger += 1;
                                else if (percentageOfBidsAtTop < percentageOfAsksAtTop)
                                        this.TotalTopBidsVSAsk_AskBigger += 1;

                                if (percentageOfBidsAtBottom > percentageOfAsksAtBottom)
                                        this.TotalBottomBidsVSAsk_BidBigger += 1;
                                else if (percentageOfBidsAtBottom < percentageOfAsksAtBottom)
                                        this.TotalBottomBidsVSAsk_AskBigger += 1;

                                if (percentageOfDataAtTop > percentageOfDataAtBottom)
                                        this.TotalDataAtTop += 1;
                                else if (percentageOfDataAtTop < percentageOfDataAtBottom)
                                        this.TotalDataAtBottom += 1;
                        }

                        public string PrintResult()
                        {
                                string resultSet = Name + " -->   " ;
                                //04/02/2012 01:00:00 - SPY (5 min):  SHORT 2 < 50 < 10 < 00 < 0
                                resultSet +=  " (" +TotalBidsVSAsk_BidBigger;
                                resultSet += (TotalBidsVSAsk_BidBigger > TotalBidsVSAsk_AskBigger)
? " > " : " < ";
                                resultSet += TotalBidsVSAsk_AskBigger + ")";

                                resultSet += " (" + TotalTopBidsVSAsk_BidBigger;
                                resultSet += (TotalTopBidsVSAsk_BidBigger >
TotalTopBidsVSAsk_AskBigger) ? " > " : " < ";
                                resultSet += TotalTopBidsVSAsk_AskBigger + ")";

                                resultSet += " (" + TotalBottomBidsVSAsk_BidBigger;
                                resultSet += (TotalBottomBidsVSAsk_BidBigger >
TotalBottomBidsVSAsk_AskBigger) ? " > " : " < ";
                                resultSet += TotalBottomBidsVSAsk_AskBigger + ")";

                                resultSet += " (" + TotalDataAtTop;
                                resultSet += (TotalDataAtTop > TotalDataAtBottom) ? " > " : " < ";
                                resultSet += TotalDataAtBottom + ")";

                                return resultSet;

                        }
                }

                public void PrintStats(int BarIndexInCollection, string Message)
                {


                        int theCurrbar = CurrentBar - BarIndexInCollection;
                        int percentageOfBids = 0;
                        int percentageOfAsks = 0;
                        string directionalChar = "";
                        string directionalTotal = "";
                        string resultTotal = "";


                        percentageOfBids = GetPercentageOfBids(BarIndexInCollection,
dataType.bidAndAskSize);
                        percentageOfAsks = 100 - percentageOfBids;
                        directionalChar = (percentageOfBids > percentageOfAsks) ? " > " : " < ";
                        directionalTotal += directionalChar;
                        resultTotal += "      (" + percentageOfBids + directionalChar +
percentageOfAsks + " -Bids/Asks)";

                        int percentageOfBidsAtTop = 0;
                        int percentageOfAsksAtTop = 0;
                        percentageOfBidsAtTop =
GetPercentageOfBidsAtTopOrBottom(BarIndexInCollection,
dataType.bidAndAskSize, Median[theCurrbar], TopOrBottomEnum.Top);
                        percentageOfAsksAtTop = 100 - percentageOfBidsAtTop;
                        directionalChar = (percentageOfBidsAtTop > percentageOfAsksAtTop) ?
" > " : " < ";
                        directionalTotal += directionalChar;
                        resultTotal += " (" + percentageOfBidsAtTop + directionalChar +
percentageOfAsksAtTop + " -Top Bids/Asks)";

                        int percentageOfBidsAtBottom = 0;
                        int percentageOfAsksAtBottom = 0;
                        percentageOfBidsAtBottom =
GetPercentageOfBidsAtTopOrBottom(BarIndexInCollection,
dataType.bidAndAskSize, Median[theCurrbar], TopOrBottomEnum.Bottom);
                        percentageOfAsksAtBottom = 100 - percentageOfBidsAtBottom;
                        directionalChar = (percentageOfBidsAtBottom >
percentageOfAsksAtBottom) ? " > " : " < ";
                        directionalTotal += directionalChar;
                        resultTotal += " (" + percentageOfBidsAtBottom + directionalChar +
percentageOfAsksAtBottom + " -Bottom Bids/Asks)";

                        int percentageOfDataAtTop = 0;
                        int percentageOfDataAtBottom = 0;
                        percentageOfDataAtTop =
GetPercentageOfDataAtTopHalfOfBar(BarIndexInCollection,
dataType.bidAndAskSize, Median[theCurrbar]);
                        percentageOfDataAtBottom = 100 - percentageOfDataAtTop;
                        directionalChar = (percentageOfDataAtTop >
percentageOfDataAtBottom) ? " > " : " < ";
                        directionalTotal += directionalChar;
                        resultTotal += " (" + percentageOfDataAtTop + directionalChar +
percentageOfDataAtBottom + " -Data Top/Bottom)";

                        //PrintAlert(Time[theCurrbar], Message + " " + directionalTotal + "
" + resultTotal);

                        if (Message == "LONG")
                        {
                                _LongStatisticHolder.AddData(percentageOfBids,percentageOfAsks,
percentageOfBidsAtTop,percentageOfAsksAtTop
                                        ,percentageOfBidsAtBottom,percentageOfAsksAtBottom,
percentageOfDataAtTop,percentageOfDataAtBottom );
                                PrintAlert(Time[theCurrbar], _LongStatisticHolder.PrintResult());
                        }
                        if (Message == "SHORT")
                        {
                                _ShortStatisticHolder.AddData(percentageOfBids,percentageOfAsks,
percentageOfBidsAtTop,percentageOfAsksAtTop
                                        ,percentageOfBidsAtBottom,percentageOfAsksAtBottom,
percentageOfDataAtTop,percentageOfDataAtBottom );
                                PrintAlert(Time[theCurrbar], _ShortStatisticHolder.PrintResult());
                        }


                }

#endregion

        #region BarAnalysis


                public enum MarketDirectionEnum{Bullish, Bearish};
                public enum DirectionalFilterEnum{VeryStrong, Normal};
                public bool IsBarAerodynamic(int BarIndex, MarketDirectionEnum
theMarketDirection, DirectionalFilterEnum theDirectionalFilterEnum)
                {
                        if (theMarketDirection == MarketDirectionEnum.Bullish)
                        {
                                if (IsTopOrBottomQuarter(BarIndex, Close[BarIndex]) == TopOrBottomEnum.Top
                                        && IsAboveOrBelowMedian(BarIndex, Open[BarIndex]) == TopOrBottomEnum.Bottom
                                        && HasMoreTailAtTopOrBottom(BarIndex, TopOrBottomEnum.Bottom)
                                        && (Open[BarIndex] < Close[BarIndex]))

                                        if (theDirectionalFilterEnum == DirectionalFilterEnum.VeryStrong)
                                        {
                                                if (IsTopOrBottomThird(BarIndex, Open[BarIndex]) ==
TopOrBottomEnum.Bottom)
                                                        return true;
                                        }
                                        else
                                                return true;
                        }
                        if (theMarketDirection == MarketDirectionEnum.Bearish)
                        {
                                if (IsTopOrBottomQuarter(BarIndex,Close[BarIndex]) == TopOrBottomEnum.Bottom
                                        && IsAboveOrBelowMedian(BarIndex, Open[BarIndex]) == TopOrBottomEnum.Top
                                        && HasMoreTailAtTopOrBottom(BarIndex, TopOrBottomEnum.Top)
                                        && (Open[BarIndex] > Close[BarIndex]))

                                        if (theDirectionalFilterEnum == DirectionalFilterEnum.VeryStrong)
                                        {
                                                if (IsTopOrBottomThird(BarIndex, Open[BarIndex]) == TopOrBottomEnum.Top)
                                                        return true;
                                        }
                                        else
                                                return true;
                        }
                        return false;
                }


                public enum TopOrBottomEnum {Top, Bottom, None};
                public TopOrBottomEnum IsTopOrBottomThird(int BarIndex, double thePrice)
                {
                        double sizeOfBar = High[BarIndex] - Low[BarIndex];

                        if (((sizeOfBar * 0.666) + Low[BarIndex]) < thePrice)
                                return TopOrBottomEnum.Top;
                        else if (((sizeOfBar * 0.333) + Low[BarIndex]) > thePrice)
                                return TopOrBottomEnum.Bottom;

                        return TopOrBottomEnum.None;
                }

                public TopOrBottomEnum IsTopOrBottomQuarter(int BarIndex, double thePrice)
                {
                        double sizeOfBar = High[BarIndex] - Low[BarIndex];
                        double quarterSize = sizeOfBar * 0.25;

                        if (((quarterSize * 3) + Low[BarIndex]) < thePrice)
                                return TopOrBottomEnum.Top;
                        else if ((quarterSize + Low[BarIndex]) > thePrice)
                                return TopOrBottomEnum.Bottom;

                        return TopOrBottomEnum.None;
                }

                public TopOrBottomEnum IsAboveOrBelowMedian(int BarIndex, double thePrice)
                {
                        if (thePrice > Median[BarIndex])
                                return TopOrBottomEnum.Top;
                        else if  (thePrice < Median[BarIndex])
                                return TopOrBottomEnum.Bottom;

                        return TopOrBottomEnum.None;
                }

                public bool HasMoreTailAtTopOrBottom(int BarIndex, TopOrBottomEnum
theTopOrBottomEnum)
                {
                        double tailAtTop = High[BarIndex] - Math.Max(Open[BarIndex],
Close[BarIndex]);
                        double tailAtBottom = Math.Min(Open[BarIndex], Close[BarIndex]) -
Low[BarIndex];

                        //Always true when there is no tail in interested direction
                        if ((theTopOrBottomEnum == TopOrBottomEnum.Top) && tailAtBottom == 0)
                                return true;

                        if ((theTopOrBottomEnum == TopOrBottomEnum.Bottom) && tailAtTop == 0)
                                return true;

                        if (theTopOrBottomEnum == TopOrBottomEnum.Top)
                                return (tailAtTop > tailAtBottom) ? true : false;

                        if (theTopOrBottomEnum == TopOrBottomEnum.Bottom)
                                return (tailAtTop < tailAtBottom) ? true : false;

                        return false;
                }

                #endregion

        #region Properties


//               [GridCategory("Parameters")]
//               [Gui.Design.DisplayName("Divisor")]
//               public int Divisor
//               {
//               get { return _Divisor; }
//               set { _Divisor = value; }
//               }

               [GridCategory("Parameters")]
               [Gui.Design.DisplayName("Version")]
               public string Version
               {
               get { return _Version; }
                }

                          [GridCategory("Parameters")]
               [Gui.Design.DisplayName("Divisor")]
               public int Divisor
               {
               get { return _Divisor; }
                }
       #endregion

   }

}