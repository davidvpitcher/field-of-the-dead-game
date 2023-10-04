<?php // get_alphonso_reward.inc.php

if (isset($_POST["playerID"])) {
    $playerID = $_POST["playerID"];
    
    $unity = true;
    $login = true;
    require_once '../../keys/storage.php';

    $sql = "SELECT received_alphonso_upgrade FROM player_stats WHERE player_id = :playerID";
    
    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':playerID', $playerID, PDO::PARAM_INT);
    $stmt->execute();

    if ($stmt->rowCount() > 0) {
        $row = $stmt->fetch(PDO::FETCH_ASSOC);
        echo "0\t" . $row["received_alphonso_upgrade"];
    } else {
        // If row not found, return default value for received_alphonso_upgrade
        echo "0\t0";
    }
} else {
    echo "2"; // missing POST variable
}
