## https://www.geeksforgeeks.org/python-program-for-topological-sorting/

## Edited by Tor-Salve Dalsgaard, UCPH

#Python program to print topological sorting of a DAG 
from collections import defaultdict 

#Class to represent a graph 
class Graph: 
	def __init__(self,vertices): 
		self.graph = defaultdict(list) #dictionary containing adjacency List 
		self.vertices = vertices
		self.V = len(vertices) #No. of vertices 

	# function to add an edge to graph 
	def addEdge(self,u,v): 
		self.graph[u].append(v) 

	# A recursive function used by topologicalSort 
	def topologicalSortUtil(self,i,v,visited,stack): 

		# Mark the current node as visited. 
		visited[i] = True

		# Recur for all the vertices adjacent to this vertex 
		for u in self.graph[v]:
			j = self.vertices.index(u)
			if visited[j] == False:
				self.topologicalSortUtil(j,u,visited,stack) 

		# Push current vertex to stack which stores result 
		stack.insert(0,v) 

	# The function to do Topological Sort. It uses recursive 
	# topologicalSortUtil() 
	def topologicalSort(self): 
		# Mark all the vertices as not visited 
		visited = [False]*self.V 
		stack =[] 

		# Call the recursive helper function to store Topological 
		# Sort starting from all vertices one by one 
		for i, v in enumerate(self.vertices): 
			if visited[i] == False:
				self.topologicalSortUtil(i,v,visited,stack) 

    # Print contents of stack 
		return stack

#This code is contributed by Neelam Yadav 
