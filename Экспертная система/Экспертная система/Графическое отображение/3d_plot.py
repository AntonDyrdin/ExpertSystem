import pandas as pd

data = pd.read_csv("grid_search_4.txt", sep=';')


#Make Plotly figure
import plotly
import plotly.graph_objs as go

markercolor = data['window_size']

fig1 = go.Scatter3d(x=data['learning_rate'],
                    y=data['number_of_epochs'],
                    z=data['Q'],
                    marker=dict(color=markercolor,
                                opacity=1,
                               colorscale=[[0.5, "rgb(250,250,250)"],
                                            [0.7, "rgb(240,240,240)"],
                                            [0.801, "rgb(230,230,230)"],
                                            [0.85, "rgb(220,220,220)"],
                                            [0.86, "rgb(210,210,210)"],
                                            [0.87, "rgb(200,200,200)"],
                                            [0.88, "rgb(100,100,100)"],
                                            [0.89, "rgb(50,50,50)"],
                                            [0.9, "rgb(25,25,25)"],
                                            [1.0, "rgb(0,0,0)"]],
                                size=15),
                    line=dict (width=0.02),
                    mode='markers')

#Make Plot.ly Layout
layout = go.Layout(scene=dict(xaxis=dict( title="learning_rate"),
                                yaxis=dict( title="number_of_epochs"),
                                zaxis=dict( title="Q")),)

#Plot and save html
plotly.offline.plot({"data": [fig1],
                     "layout": layout},
                     auto_open=True,
                     filename=("4DPlot.html"))

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