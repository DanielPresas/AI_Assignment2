# Improvements - Assignment 2

## INFR 4320U - AI For Games

## Daniel Presas - 100699431

1. Ability to handle multiple sizes of board

    Tic-tac-toe is normally played on a 3x3 grid, but I made it possible to be played on any size grid board. However, the bigger the board is, the longer the AI takes to compute and resolve the best possible move on the board in a reasonable amount of time. Which leads to the second optimization...

2. Alpha-beta pruning + concurrency

    The AI runs on multiple threads to speed up solving through the tree, and uses alpha-beta pruning in the process to discard any nodes that should not be visited at all. 
