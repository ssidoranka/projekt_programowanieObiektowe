﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

// Класс Самолет
public class Flight
{
    public int FlightId { get; set; }
    public DateTime Date { get; set; }
    public string DepartureCity { get; set; }
    public string DestinationCity { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public List<Passenger> Passengers { get; set; } = new List<Passenger>();
}

// Класс пассажир
public class Passenger
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ReservationCode { get; set; }
}

// Управление данными с помощью CRUD
public abstract class CrudRepository<T>
{
    protected List<T> Items { get; set; } = new List<T>();

    public void Create(T item)
    {
        Items.Add(item);
    }

    public List<T> Read()
    {
        return Items;
    }

    public void Update(T oldItem, T newItem)
    {
        int index = Items.IndexOf(oldItem);
        if (index != -1)
        {
            Items[index] = newItem;
        }
    }

    public void Delete(T item)
    {
        Items.Remove(item);
    }
}

// Управление самолетами 
public class FlightRepository : CrudRepository<Flight>
{
   
    public List<Flight> SearchFlights(string departureCity)
    {
        return Items.FindAll(f => f.DepartureCity.Equals(departureCity, StringComparison.OrdinalIgnoreCase)
            && f.AvailableSeats > 0);
    }
}

// Дополнительный пустой класс
public class PassengerRepository : CrudRepository<Passenger>
{
    
}

class Program
{
    static void Main()
    {
        FlightRepository flightRepository = new FlightRepository();
        PassengerRepository passengerRepository = new PassengerRepository();

        // Чтение данных о самолетах из текстового файла
        LoadFlightsFromFile("D:\\proga\\ConsoleApp3\\ConsoleApp3\\flights.txt", flightRepository);

        bool searchAgain = true;

        while (searchAgain)
        {
            Console.WriteLine("\nAvailable Flights:");
            DisplayAvailableFlights(flightRepository.Read());

            Console.Write("\nEnter the Flight ID to book or cancel a seat: ");
            if (int.TryParse(Console.ReadLine(), out int selectedFlightId))
            {
                Flight selectedFlight = flightRepository.Read().FirstOrDefault(f => f.FlightId == selectedFlightId);

                if (selectedFlight != null)
                {
                    Console.Write("Do you want to book (B) or cancel (C) a seat? ");
                    string action = Console.ReadLine().ToUpper();

                    if (action == "B" && selectedFlight.AvailableSeats > 0)
                    {
                        // Осуществление резезрвации
                        Console.Write("Enter First Name: ");
                        string firstName = Console.ReadLine();

                        Console.Write("Enter Last Name: ");
                        string lastName = Console.ReadLine();

                        Passenger selectedPassenger = new Passenger { FirstName = firstName, LastName = lastName, ReservationCode = Guid.NewGuid().ToString() };
                        passengerRepository.Create(selectedPassenger);

                        selectedFlight.Passengers.Add(selectedPassenger);
                        selectedFlight.AvailableSeats--;

                        decimal amountToPay = selectedFlight.Price;
                        Console.WriteLine($"\nReservation successful. Passenger: {selectedPassenger.FirstName} {selectedPassenger.LastName}, Flight ID: {selectedFlight.FlightId}, Remaining Seats: {selectedFlight.AvailableSeats}, Amount to Pay: {amountToPay}");

                        // Данные о пассажирах записываются в текстовый файл
                        SavePassengerToFile("D:\\proga\\ConsoleApp3\\ConsoleApp3\\passengers.txt", selectedPassenger);
                    }
                    else if (action == "C" && selectedFlight.Passengers.Count > 0)
                    {
                        // ОТмена резервации
                        Console.Write("Enter Passenger's Reservation Code to cancel: ");
                        string reservationCodeToCancel = Console.ReadLine();

                        Passenger passengerToCancel = selectedFlight.Passengers.FirstOrDefault(p => p.ReservationCode == reservationCodeToCancel);

                        if (passengerToCancel != null)
                        {
                            selectedFlight.Passengers.Remove(passengerToCancel);
                            selectedFlight.AvailableSeats++;
                            Console.WriteLine("Reservation canceled successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Invalid Reservation Code or no reservations for the specified code.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid action or no available seats/reservations.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Flight ID.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input for Flight ID.");
            }

            Console.Write("\nDo you want to search for flights again? (Y/N): ");
            string userResponse = Console.ReadLine();

            searchAgain = userResponse.Equals("Y", StringComparison.OrdinalIgnoreCase);
        }
    }

    // Записывает данные о пассажирах в текстовый файл
    static void SavePassengerToFile(string filePath, Passenger passenger)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{passenger.FirstName},{passenger.LastName},{passenger.ReservationCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving passenger information to the file: {filePath}\n{ex.Message}");
        }
    }

    // Чтение данных о пассажирах из файла
    static void LoadPassengersFromFile(string filePath, PassengerRepository passengerRepository)
    {
        try
        {
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] fields = line.Split(',');

                    if (fields.Length == 3) 
                    {
                        Passenger passenger = new Passenger
                        {
                            FirstName = fields[0],
                            LastName = fields[1],
                            ReservationCode = fields[2]
                        };

                        passengerRepository.Create(passenger);
                    }
                    else
                    {
                        Console.WriteLine($"Invalid data format in the passenger file: {filePath}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading passengers from the file: {filePath}\n{ex.Message}");
           
        }
    }

    static void DisplayAvailableFlights(List<Flight> flights)
    {
        foreach (var flight in flights)
        {
            Console.WriteLine($"Flight ID: {flight.FlightId}, Departure: {flight.DepartureCity}, Destination: {flight.DestinationCity}, Date: {flight.Date}, Available Seats: {flight.AvailableSeats}, Price: {flight.Price}");
        }
    }

    static void LoadFlightsFromFile(string filePath, FlightRepository flightRepository)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] fields = line.Split(',');

                if (fields.Length == 6)
                {
                    Flight flight = new Flight
                    {
                        FlightId = int.Parse(fields[0]),
                        Date = DateTime.ParseExact(fields[1], "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        DepartureCity = fields[2],
                        DestinationCity = fields[3],
                        AvailableSeats = int.Parse(fields[4]),
                        Price = decimal.Parse(fields[5], CultureInfo.InvariantCulture)
                    };

                    flightRepository.Create(flight);
                }
                else
                {
                    Console.WriteLine($"Invalid data format in the file: {filePath}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading flights from the file: {filePath}\n{ex.Message}");
        }
    }
}