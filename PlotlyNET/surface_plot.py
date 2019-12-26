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
log("СКРИПТ surface_plot ЗАПУЩЕН...") 


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


import pandas as pd

z_data = pd.read_csv(args.file_path, sep=';')

import pandas as pd
import numpy as np
from plotly.offline import download_plotlyjs, init_notebook_mode, plot
import plotly.graph_objs as go

data = [
    go.Surface(
        z=z_data.as_matrix()
    )
]
layout = go.Layout(
    title=args.file_path,
    autosize=True,

    margin=dict(
        l=65,
        r=50,
        b=65,
        t=90
    )
)
fig = go.Figure(data=data, layout=layout)  
plot(fig)

#sh_0, sh_1 = z.shape
#x, y = np.linspace(0, 1, sh_0), np.linspace(0, 1, sh_1)
#fig = go.Figure(data=[go.Surface(z=z, x=x, y=y)])
#fig.update_layout(title='Mt Bruno Elevation', autosize=False,
#                  width=500, height=500,
#                  margin=dict(l=65, r=50, b=65, t=90))