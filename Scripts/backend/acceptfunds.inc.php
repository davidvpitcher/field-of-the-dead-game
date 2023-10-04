<?php // acceptfunds.inc.php

if (isset($_POST["saleID"])) {
    $unity = true;
    $login = true;

    $saleID = intval($_POST["saleID"]);
    $itemprice = intval($_POST["price"]);
    $username = $_POST["username"];
    $quantity = intval($_POST["quantity"]);
    $level = intval($_POST["level"]);

    require_once '../../keys/storage.php';

    // Prepare and bind the SQL statement
    $stmt = $conn->prepare("SELECT * FROM market_sales WHERE id = ? AND seller = ? AND price = ? AND quantity = ? AND level = ?");
    $stmt->bind_param("isiis", $saleID, $username, $itemprice, $quantity, $level);
    
    $stmt->execute();
    $result = $stmt->get_result();
    
    if ($result->num_rows > 0) {
        // Deleting the matching entry
        $delete_stmt = $conn->prepare("DELETE FROM market_sales WHERE id = ?");
        $delete_stmt->bind_param("i", $saleID);
        $delete_stmt->execute();
        echo "0";
        $delete_stmt->close();
    } else {
        echo "5Invalid Sale";
    }
    
    // Close the connection
    $stmt->close();
    $conn->close();

    exit();
} else {
    echo "1";
    exit();
}
