<?php // fetch_vehicle_stat.php

if (isset($_POST["userID"])) {
    $unity = true;
    $login = true;
    $userID = $_POST["userID"];
    
    require_once '../../keys/storage.php';  // Assuming PDO is initialized here as $pdo

    try {
        // Select only relevant columns
        $sql = "SELECT ACCEL, SPEED, TURN, HEALTH, BRAKE FROM player_stats WHERE player_id = :userID";
        $stmt = $pdo->prepare($sql);
        $stmt->bindValue(':userID', $userID);
        $stmt->execute();
        $stats = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($stats) {
            echo json_encode(array("status" => "success", "data" => $stats));
        } else {
            // If no stats found for the user, insert default stats and return them
            $defaultStats = [
                'ACCEL' => 1,
                'SPEED' => 1,
                'TURN' => 1,
                'HEALTH' => 1,
                'BRAKE' => 1
            ];
            
            // Insert default stats
            $sql = "INSERT INTO player_stats (player_id, ACCEL, SPEED, TURN, HEALTH, BRAKE) 
                    VALUES (:userID, :ACCEL, :SPEED, :TURN, :HEALTH, :BRAKE)";
            $stmt = $pdo->prepare($sql);
            $stmt->bindValue(':userID', $userID);
            foreach ($defaultStats as $key => $value) {
                $stmt->bindValue(":$key", $value);
            }
            $stmt->execute();

            echo json_encode(array("status" => "success", "data" => $defaultStats));
        }
    } catch (Exception $e) {
        echo json_encode(array("status" => "error", "message" => $e->getMessage()));
    }
} else {
    echo json_encode(array("status" => "error", "message" => "Missing userID parameter"));
}
