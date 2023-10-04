<?php // lookuppostings.inc.php

const ERROR_INVALID_CODE = "7";
const ERROR_EXECUTION_FAILED = "1";

if (isset($_POST["code"])) {
    try {
        $unity = true;
        $login = true;
        require_once '../../keys/storage.php';

        $pdo->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        $username = $_POST["playername"];
        
        $sql = "SELECT name, MIN(price) as cheapest_price, SUM(quantity) as total_quantity FROM market_items WHERE seller != :playerName GROUP BY name";
        $statement = $pdo->prepare($sql);
        $statement->bindParam(':playerName', $username, PDO::PARAM_STR);
        $statement->execute();
        $results = $statement->fetchAll(PDO::FETCH_ASSOC);
        
        $totalstring = "";
        
        foreach ($results as $row) {
            $totalstring .= $row['name'] . ":" . $row['cheapest_price'] . ":" . $row['total_quantity'] . ";";
        }
        
        echo "0" . $totalstring;
        
    } catch (PDOException $e) {
        error_log("Database error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    } catch (Exception $e) {
        error_log("General error: " . $e->getMessage());
        echo ERROR_EXECUTION_FAILED;
    }
} else {
    echo ERROR_INVALID_CODE;
}

exit();
?>
