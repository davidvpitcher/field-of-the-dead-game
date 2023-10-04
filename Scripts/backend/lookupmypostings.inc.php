<?php


if (isset($_POST["code"])) {


    $unity = true;
    $login = true;
    
$username = $_POST["playername"];
    
    require_once '../../keys/storage.php';
    



    if ($conn->connect_error) {
        echo "Error connecting to the database.";
        die();
    }




    $query = "SELECT name, level, quantity, price, seller, id FROM market_sales WHERE seller = '".$username."';";
    $result = $conn->query($query);
$totalstring = "";
    if ($result->num_rows > 0) {
       


        while ($row = $result->fetch_assoc()) {
            $totalstring = $totalstring."," . $row["name"] . "," . $row["level"] . "," . $row["quantity"] . "," . $row["price"] .  "," . $row["id"];
        }

echo "0".$totalstring;

exit();


    } else {
        echo "1";
        exit();
    }











    echo "5";
   
   // echo "0".$totalstring;

exit();


} else {


    echo "7";
    exit();
}