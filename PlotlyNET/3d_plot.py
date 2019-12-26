import os
logPath = "log.txt"
logErrorPath = "log_error.txt"
import sys
try:
    sys.stderr = open(logErrorPath, 'w')
    logFile = open(logPath,"w")
    logFile.write(logPath)
    logFile.close()
except:
    print("не удалось открыть файл логгирования ",logPath)
def log(s):
    try:
        logFile = open(logPath,"a")
        logFile.write((str)(s) + '\n')
        logFile.close()
        print(s)
    except:
        print(s)
log(logPath)
######################################################################
log("СКРИПТ 3d_plot ЗАПУЩЕН...") 

import argparse
import pandas as pd
def createParser():
    parser = argparse.ArgumentParser()
    parser.add_argument('--file_path',type=str)
    parser.add_argument('--x',type=str)
    parser.add_argument('--y',type=str)
    parser.add_argument('--z',type=str)
    parser.add_argument('--marker',type=str)
    return parser
parser = createParser()
args = parser.parse_args()


fileName = args.file_path
print( "X = " + args.x)
print( "Y = " + args.y)
print( "Z = " + args.z)
print( "Marker = " + args.marker)

data = pd.read_csv(fileName, sep=';')


#Make Plotly figure
import plotly
import plotly.graph_objs as go

markercolor = data[ args.marker]

fig1 = go.Scatter3d(x=data[args.x],
                    y=data[args.y],
                    z=data[args.z],
                    marker=dict(color=markercolor,
                                opacity=1,
                                size=10),
                    line=dict (width=0.02),
                    mode='markers')

#Make Plot.ly Layout
layout = go.Layout(scene=dict(xaxis=dict( title=args.x),
                                yaxis=dict( title=args.y),
                                zaxis=dict( title=args.z)),)

#Plot and save html
plotly.offline.plot({"data": [fig1],
                     "layout": layout},
                     auto_open=True,
                     filename=(fileName + "_4DPlot.html"))

#'aggrnyl', 'agsunset', 'algae', 'amp', 'armyrose', 'balance',
#             'blackbody', 'bluered', 'blues', 'blugrn', 'bluyl', 'brbg',
#             'brwnyl', 'bugn', 'bupu', 'burg', 'burgyl', 'cividis', 'curl',
#             'darkmint', 'deep', 'delta', 'dense', 'earth', 'edge', 'electric',
#             'emrld', 'fall', 'geyser', 'gnbu', 'gray', 'greens', 'greys',
#             'haline', 'hot', 'hsv', 'ice', 'icefire', 'inferno', 'jet',
#             'magenta', 'magma', 'matter', 'mint', 'mrybm', 'mygbm', 'oranges',
#             'orrd', 'oryel', 'peach', 'phase', 'picnic', 'pinkyl', 'piyg',
#             'plasma', 'plotly3', 'portland', 'prgn', 'pubu', 'pubugn', 'puor',
#             'purd', 'purp', 'purples', 'purpor', 'rainbow', 'rdbu', 'rdgy',
#             'rdpu', 'rdylbu', 'rdylgn', 'redor', 'reds', 'solar', 'spectral',
#             'speed', 'sunset', 'sunsetdark', 'teal', 'tealgrn', 'tealrose',
#             'tempo', 'temps', 'thermal', 'tropic', 'turbid', 'twilight',
#             'viridis', 'ylgn', 'ylgnbu', 'ylorbr', 'ylorrd