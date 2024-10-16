using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        int numberOfElements = 500000; // Kích thước mảng là 500.000
        int iterations = 1000;
        Random random = new Random();
        int chunkSize = 80; // Kích thước chunk được đặt là 80

        // Danh sách lưu thời gian thực hiện cho từng thuật toán
        List<double> selectionSortTimes = new List<double>();
        List<double> quickSortTimes = new List<double>();
        List<double> shellSortTimes = new List<double>();
        List<double> mergeSortTimes = new List<double>();
        List<double> bubbleSortTimes = new List<double>();

        // Thống kê số lần chạy nhanh hơn
        int selectionSortWins = 0;
        int quickSortWins = 0;
        int shellSortWins = 0;
        int mergeSortWins = 0;
        int bubbleSortWins = 0;

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            // Bước 1: Tạo danh sách 500.000 phần tử
            List<int> originalList = Enumerable.Range(0, numberOfElements).Select(x => random.Next(1, 1000000)).ToList();

            // Bước 2: Clone danh sách thành 5 danh sách
            List<int>[] lists = new List<int>[5];
            for (int j = 0; j < 5; j++)
            {
                lists[j] = new List<int>(originalList);
            }

            // Bước 3: Bấm giờ từ thời điểm này
            Stopwatch stopwatch = new Stopwatch();

            // Bước 4: Chạy các thuật toán Sort song song
            Parallel.Invoke(
                () =>
                {
                    stopwatch.Start();
                    SortAndMerge(lists[0], SelectionSort, chunkSize);
                    stopwatch.Stop();
                    selectionSortTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                },
                () =>
                {
                    stopwatch.Restart();
                    SortAndMerge(lists[1], QuickSort, chunkSize);
                    stopwatch.Stop();
                    quickSortTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                },
                () =>
                {
                    stopwatch.Restart();
                    SortAndMerge(lists[2], ShellSort, chunkSize);
                    stopwatch.Stop();
                    shellSortTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                },
                () =>
                {
                    stopwatch.Restart();
                    SortAndMerge(lists[3], MergeSort, chunkSize);
                    stopwatch.Stop();
                    mergeSortTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                },
                () =>
                {
                    stopwatch.Restart();
                    SortAndMerge(lists[4], BubbleSort, chunkSize);
                    stopwatch.Stop();
                    bubbleSortTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
                }
            );

            // Thống kê số lần thuật toán chạy nhanh hơn
            double selectionSortTime = selectionSortTimes.Last();
            double quickSortTime = quickSortTimes.Last();
            double shellSortTime = shellSortTimes.Last();
            double mergeSortTime = mergeSortTimes.Last();
            double bubbleSortTime = bubbleSortTimes.Last();

            double[] times = { selectionSortTime, quickSortTime, shellSortTime, mergeSortTime, bubbleSortTime };
            double minTime = times.Min();

            if (selectionSortTime == minTime) selectionSortWins++;
            if (quickSortTime == minTime) quickSortWins++;
            if (shellSortTime == minTime) shellSortWins++;
            if (mergeSortTime == minTime) mergeSortWins++;
            if (bubbleSortTime == minTime) bubbleSortWins++;

            // Hiện thị thanh tiến độ
            DisplayProgress(iteration + 1, iterations);
        }

        // Bước 5: Tính tổng thời gian và thời gian trung bình cho từng thuật toán
        Console.WriteLine("Execution time of each algorithm (ms):");
        Console.WriteLine($"Selection Sort: {selectionSortTimes.Sum()} (Avg: {selectionSortTimes.Average()} ms)");
        Console.WriteLine($"QuickSort: {quickSortTimes.Sum()} (Avg: {quickSortTimes.Average()} ms)");
        Console.WriteLine($"Shell Sort: {shellSortTimes.Sum()} (Avg: {shellSortTimes.Average()} ms)");
        Console.WriteLine($"Merge Sort: {mergeSortTimes.Sum()} (Avg: {mergeSortTimes.Average()} ms)");
        Console.WriteLine($"Bubble Sort: {bubbleSortTimes.Sum()} (Avg: {bubbleSortTimes.Average()} ms)");

        // In ra số lần mỗi thuật toán chạy nhanh hơn
        Console.WriteLine("\nNumber of times the algorithm runs faster:");
        Console.WriteLine($"Selection Sort: {selectionSortWins}");
        Console.WriteLine($"QuickSort: {quickSortWins}");
        Console.WriteLine($"Shell Sort: {shellSortWins}");
        Console.WriteLine($"Merge Sort: {mergeSortWins}");
        Console.WriteLine($"Bubble Sort: {bubbleSortWins}");
    }

    // Phương thức sắp xếp các chunk và gộp lại
    static void SortAndMerge(List<int> arr, Action<List<int>> sortFunc, int chunkSize)
    {
        List<List<int>> chunks = new List<List<int>>();

        // Chia thành các chunk
        for (int start = 0; start < arr.Count; start += chunkSize)
        {
            int size = Math.Min(chunkSize, arr.Count - start);
            List<int> chunk = arr.Skip(start).Take(size).ToList();
            chunks.Add(chunk);
        }

        // Sắp xếp song song từng chunk
        Parallel.ForEach(chunks, chunk =>
        {
            sortFunc(chunk);
        });

        // Gộp các chunk đã sắp xếp
        arr.Clear();
        foreach (var chunk in chunks)
        {
            arr.AddRange(chunk);
        }
    }

    static void DisplayProgress(int current, int total)
    {
        Console.Write("\rProgress of completed iterations: [");
        int progressWidth = 50;
        int position = (int)((double)current / total * progressWidth);
        for (int i = 0; i < progressWidth; i++)
        {
            if (i < position)
                Console.Write("=");
            else
                Console.Write(" ");
        }
        Console.Write($"] {current}/{total} ({(double)current / total * 100:0.00}%)");
    }

    // Các thuật toán sắp xếp
    static void SelectionSort(List<int> arr)
    {
        for (int i = 0; i < arr.Count - 1; i++)
        {
            int minIndex = i;
            for (int j = i + 1; j < arr.Count; j++)
            {
                if (arr[j] < arr[minIndex])
                    minIndex = j;
            }
            int temp = arr[minIndex];
            arr[minIndex] = arr[i];
            arr[i] = temp;
        }
    }

    static void QuickSort(List<int> arr)
    {
        QuickSort(arr, 0, arr.Count - 1);
    }

    static void QuickSort(List<int> arr, int low, int high)
    {
        if (low < high)
        {
            int pi = Partition(arr, low, high);
            QuickSort(arr, low, pi - 1);
            QuickSort(arr, pi + 1, high);
        }
    }

    static int Partition(List<int> arr, int low, int high)
    {
        int pivot = arr[high];
        int i = (low - 1);
        for (int j = low; j < high; j++)
        {
            if (arr[j] < pivot)
            {
                i++;
                int temp = arr[i];
                arr[i] = arr[j];
                arr[j] = temp;
            }
        }
        int temp1 = arr[i + 1];
        arr[i + 1] = arr[high];
        arr[high] = temp1;
        return i + 1;
    }

    static void ShellSort(List<int> arr)
    {
        int n = arr.Count;
        for (int gap = n / 2; gap > 0; gap /= 2)
        {
            for (int i = gap; i < n; i++)
            {
                int temp = arr[i];
                int j;
                for (j = i; j >= gap && arr[j - gap] > temp; j -= gap)
                {
                    arr[j] = arr[j - gap];
                }
                arr[j] = temp;
            }
        }
    }

    static void MergeSort(List<int> arr)
    {
        MergeSort(arr, 0, arr.Count - 1);
    }

    static void MergeSort(List<int> arr, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;
            MergeSort(arr, left, mid);
            MergeSort(arr, mid + 1, right);
            Merge(arr, left, mid, right);
        }
    }

    static void Merge(List<int> arr, int left, int mid, int right)
    {
        int n1 = mid - left + 1;
        int n2 = right - mid;

        List<int> L = new List<int>(n1);
        List<int> R = new List<int>(n2);

        for (int i = 0; i < n1; ++i)
            L.Add(arr[left + i]);
        for (int j = 0; j < n2; ++j)
            R.Add(arr[mid + 1 + j]);

        int k = left, i1 = 0, j1 = 0;

        while (i1 < n1 && j1 < n2)
        {
            if (L[i1] <= R[j1])
            {
                arr[k++] = L[i1++];
            }
            else
            {
                arr[k++] = R[j1++];
            }
        }

        while (i1 < n1)
        {
            arr[k++] = L[i1++];
        }

        while (j1 < n2)
        {
            arr[k++] = R[j1++];
        }
    }

    static void BubbleSort(List<int> arr)
    {
        int n = arr.Count;
        for (int i = 0; i < n - 1; i++)
            for (int j = 0; j < n - i - 1; j++)
                if (arr[j] > arr[j + 1])
                {
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                }
    }
}
