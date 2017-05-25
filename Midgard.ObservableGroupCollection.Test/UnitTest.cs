using System;
using System.Collections.ObjectModel;
using Xunit;

namespace Midgard.ObservableGroupCollection.Test
{
    public class UnitTest
    {
        [Fact]
        public void TestInitilisation()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            Assert.Equal(3, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('H', grouping[1].Key);
            Assert.Equal('M', grouping[2].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(1, grouping[1].Count);
            Assert.Equal(3, grouping[2].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Hans", grouping[1][0]);

            Assert.Equal("Mark", grouping[2][0]);
            Assert.Equal("Martin", grouping[2][1]);
            Assert.Equal("Michael", grouping[2][2]);
        }

        [Fact]
        public void TestAddWithoutNewGroup()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable.Add("Holgar");

            Assert.Equal(3, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('H', grouping[1].Key);
            Assert.Equal('M', grouping[2].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(2, grouping[1].Count);
            Assert.Equal(3, grouping[2].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Hans", grouping[1][0]);
            Assert.Equal("Holgar", grouping[1][1]);

            Assert.Equal("Mark", grouping[2][0]);
            Assert.Equal("Martin", grouping[2][1]);
            Assert.Equal("Michael", grouping[2][2]);
        }
        [Fact]
        public void TestAddWithNewGroup()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable.Add("Kim");

            Assert.Equal(4, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('H', grouping[1].Key);
            Assert.Equal('K', grouping[2].Key);
            Assert.Equal('M', grouping[3].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(1, grouping[1].Count);
            Assert.Equal(1, grouping[2].Count);
            Assert.Equal(3, grouping[3].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Hans", grouping[1][0]);

            Assert.Equal("Kim", grouping[2][0]);

            Assert.Equal("Mark", grouping[3][0]);
            Assert.Equal("Martin", grouping[3][1]);
            Assert.Equal("Michael", grouping[3][2]);
        }

        [Fact]
        public void TestDeleteElementWithoutDelitingGroup()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable.Remove("Martin");

            Assert.Equal(3, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('H', grouping[1].Key);
            Assert.Equal('M', grouping[2].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(1, grouping[1].Count);
            Assert.Equal(2, grouping[2].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Hans", grouping[1][0]);

            Assert.Equal("Mark", grouping[2][0]);
            Assert.Equal("Michael", grouping[2][1]);
        }

        [Fact]
        public void TestDeleteLastElementOfGroup()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable.Remove("Hans");

            Assert.Equal(2, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('M', grouping[1].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(3, grouping[1].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Mark", grouping[1][0]);
            Assert.Equal("Martin", grouping[1][1]);
            Assert.Equal("Michael", grouping[1][2]);
        }

        [Fact]
        public void TestReplaceElement()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable[0] = "Odin";

            Assert.Equal(3, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('M', grouping[1].Key);
            Assert.Equal('O', grouping[2].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(3, grouping[1].Count);
            Assert.Equal(1, grouping[2].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Mark", grouping[1][0]);
            Assert.Equal("Martin", grouping[1][1]);
            Assert.Equal("Michael", grouping[1][2]);

            Assert.Equal("Odin", grouping[2][0]);
        }

        [Fact]
        public void TestClear()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable.Clear();

            Assert.Equal(0, grouping.Count);
        }


        [Fact]
        public void TestMove()
        {
            var observable = new ObservableCollection<string>(new[] { "Hans", "Mark", "Albert", "Michael", "Martin", "Achim" });
            var grouping = observable.AsObservableGrouping(x => x[0]);

            observable.Move(2, 5);

            Assert.Equal(3, grouping.Count);

            Assert.Equal('A', grouping[0].Key);
            Assert.Equal('H', grouping[1].Key);
            Assert.Equal('M', grouping[2].Key);

            Assert.Equal(2, grouping[0].Count);
            Assert.Equal(1, grouping[1].Count);
            Assert.Equal(3, grouping[2].Count);

            Assert.Equal("Achim", grouping[0][0]);
            Assert.Equal("Albert", grouping[0][1]);

            Assert.Equal("Hans", grouping[1][0]);

            Assert.Equal("Mark", grouping[2][0]);
            Assert.Equal("Martin", grouping[2][1]);
            Assert.Equal("Michael", grouping[2][2]);
        }

    }
}
