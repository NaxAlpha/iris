Imports System.IO
Imports Accord.Neuro
Imports Accord.Neuro.Learning

Module Program

	Public Function LoadData(data As String, labels As List(Of String)) As Double()()
		Using f = New StreamReader(data)
			Dim temp As New List(Of Double())
			While Not f.EndOfStream
				Dim ln = f.ReadLine()
				Dim lnx = ln.Split(","c)
				Dim last = lnx.Last()
				If Not labels.Contains(last) Then
					labels.Add(last)
				End If
				lnx(lnx.Length - 1) = labels.IndexOf(last)
				temp.Add(lnx.Select(Function(s) Double.Parse(s)).ToArray())
			End While
			Return temp.ToArray()
		End Using
	End Function

	Function AsOutput(out As Integer, count As Integer) As Double()
		Dim x = New Double(count - 1) {}
		x(out) = 1
		Return x
	End Function

	Sub Randomize(data As Double()(), n As Integer)
		Dim r As New Random
		For i = 0 To n
			For j = 0 To data.Length - 1
				Dim x = r.Next(0, data.Length)
				Dim y = r.Next(0, data.Length)
				Dim z = data(x)
				data(x) = data(y)
				data(y) = z
			Next
		Next
	End Sub

	Sub Main()
		Dim lbls As New List(Of String)
		Dim data = LoadData("iris.txt", lbls)

		Randomize(data, 1000)

		Dim inputs = data.Select(Function(d) d.Take(d.Count - 1).ToArray()).ToArray()
		Dim outputs = data.Select(Function(d) AsOutput(d.Last, lbls.Count)).ToArray()

		Dim trainX = inputs.Take(100).ToArray()
		Dim trainY = outputs.Take(100).ToArray()
		Dim network = Train(trainX, trainY)

		Dim testX = inputs.Skip(100).ToArray()
		Dim testY = outputs.Skip(100).ToArray()

		Dim nTrue = 0

		For i = 0 To testX.Length - 1
			Dim x = testX(i)
			Dim y0 = testY(i)
			Dim y1 = network.Compute(x)
			Dim y2 = AsOutput(y1.ToList.IndexOf(y1.Max), 3)
			If Not y0.SequenceEqual(y2) Then
				'Console.WriteLine(False)
			Else
				nTrue += 1
			End If
		Next

		Console.WriteLine(nTrue / testX.Length)

		Console.ReadKey(True)
	End Sub

	Function Split(data As Double()(), n As Integer) As Double()()()
		Dim out = New Double(n - 1)()() {}
		Dim sz = data.Count / n
		For i = 0 To n - 1
			out(i) = data.Skip(i * sz).Take(sz).ToArray()
		Next
		Return out
	End Function

	Function Train(inputs As Double()(), outputs As Double()()) As Network
		Dim func As New BipolarSigmoidFunction()
		Dim network As New ActivationNetwork(func, 4, 1, 3)
		Dim teacher As New LevenbergMarquardtLearning(network, True)
		Dim j = 0
		Dim er = Double.PositiveInfinity
		Dim pr = er
		Dim ix = Split(inputs, 10)
		Dim iy = Split(outputs, 10)
		Do
			For i = 0 To 10
				pr = er
				er = teacher.RunEpoch(inputs, outputs)
				Console.WriteLine(er)
			Next
			j += 1
			'If j = ix.Length Then j = 0
		Loop Until Math.Abs(pr - er) < math.abs(pr) * 10 ^ -10
		Return network
	End Function

End Module
