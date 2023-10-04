<?php // cancelposting.inc.php

if (isset($_POST["code"])) {
    $unity = true;
    $login = true;
    require_once '../../keys/storage.php';

    if ($conn->connect_error) {
        echo "Error connecting to the database.";
        exit();
    }

    $saleID = intval($_POST["saleID"]);
    $playerName = $_POST["username"];

    $stmt = $conn->prepare("SELECT `seller`, `quantity` FROM `market_items` WHERE id = ?");
    $stmt->bind_param("i", $saleID);
    $stmt->execute();
    $result = $stmt->get_result();

    if ($result->num_rows > 0) {
        $row = $result->fetch_assoc();
        $sellername = $row['seller'];
        $salequantity = $row['quantity'];

        if ($sellername !== $playerName) {
            echo "3".$sellername."///".$playerName."///".$saleID;
            exit();
        }

        // Deleting the entry
        $delete_stmt = $conn->prepare("DELETE FROM market_items WHERE id = ?");
        $delete_stmt->bind_param("i", $saleID);
        $result = $delete_stmt->execute();

        echo $result ? "0".$salequantity : "1";
        $delete_stmt->close();
    } else {
        echo "2";
    }

    $stmt->close();
    $conn->close();
    exit();
} else {
    echo "2";
    exit();
}
