<?php // lookupmyunsoldpostings.inc.php

if (isset($_POST["code"])) {
    $unity = true;
    $login = true;
    $username = $_POST["playername"];
    require_once '../../keys/storage.php';

    if ($conn->connect_error) {
        echo "Error connecting to the database.";
        exit();
    }

    // Using prepared statements to prevent SQL injection
    $stmt = $conn->prepare("SELECT id, name, seller, quantity, price, level FROM market_items WHERE seller = ?");
    $stmt->bind_param("s", $username);
    $stmt->execute();
    $result = $stmt->get_result();

    $totalstring = "";

    if ($result->num_rows > 0) {
        while ($row = $result->fetch_assoc()) {
            $totalstring = $totalstring . "|" . $row["id"] . "," . $row["name"] . "," . $row["quantity"] . "," . $row["price"] . "," . $row["level"];
        }
        echo "0" . $totalstring;
    } else {
        echo "1";
    }
    $stmt->close();
    $conn->close();
    exit();
} else {
    echo "1";
    exit();
}
